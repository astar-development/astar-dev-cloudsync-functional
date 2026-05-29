using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Sync.Pipeline;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using PersistenceAccountId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.AccountId;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed partial class SyncService(IAuthService authService, ISyncRuleRepository syncRuleRepository, ISyncedItemRepository syncedItemRepository, ISyncRepository syncRepository, IRemoteFolderEnumerator remoteFolderEnumerator, IRemoteDeletionDetector remoteDeletionDetector, ILocalDeletionDetector localDeletionDetector, IDownloadJobBuilder downloadJobBuilder, ILocalChangeDetector localChangeDetector, IJobExecutor jobExecutor, ILogger<SyncService> logger) : ISyncService
{
    private sealed record SyncContext(OneDriveAccount Account, string AccessToken, IReadOnlyList<SyncRule> SyncRules, Dictionary<string, SyncedItemEntity> SyncedItemsMap, List<DeltaItem> RemoteItems);

    /// <inheritdoc />
    public event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;

    /// <inheritdoc />
    public event EventHandler<JobCompletedEventArgs>? JobCompleted;

    /// <inheritdoc />
    public event EventHandler<SyncConflict>? ConflictDetected;

    /// <inheritdoc />
    public Task<Result<Unit, SyncError>> SyncAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default)
    {
        LogSyncStarted(logger, account.AccountId.Value);
        RaiseSyncProgress(account.AccountId.Value, "Acquiring token...", 0, 0, SyncState.Syncing);

        return authService.AcquireTokenSilentAsync(account.AccountId.Value, cancellationToken)
            .MatchAsync(
                auth => LoadContextAsync(account, auth.AccessToken, cancellationToken)
                    .BindAsync(ctx => EnumerateRemoteAsync(ctx, cancellationToken))
                    .BindAsync(ctx => DetectRemoteDeletionsAsync(ctx, cancellationToken))
                    .BindAsync(ctx => DetectLocalDeletionsAsync(ctx, cancellationToken))
                    .BindAsync(ctx => ExecuteJobsAsync(ctx, cancellationToken)),
                error =>
                {
                    LogSyncFailed(logger, account.AccountId.Value, error.Message);
                    return Task.FromResult<Result<Unit, SyncError>>(new Fail<Unit, SyncError>(SyncErrorFactory.AuthFailed(error)));
                });
    }

    /// <inheritdoc />
    public Task<Result<Unit, SyncError>> ResolveConflictAsync(SyncConflict conflict, ConflictPolicy policy, CancellationToken cancellationToken = default) =>
        syncRepository.ResolveConflictAsync(new Persistence.ValueObjects.SyncConflictId(conflict.Id.Value), cancellationToken)
            .MatchAsync(
                _ => (Result<Unit, SyncError>)new Ok<Unit, SyncError>(Unit.Default),
                error =>
                {
                    LogResolveFailed(logger, conflict.Id.Value, error.Message);
                    return (Result<Unit, SyncError>)new Fail<Unit, SyncError>(SyncErrorFactory.StorageFailed(error));
                });

    private async Task<Result<SyncContext, SyncError>> LoadContextAsync(OneDriveAccount account, string accessToken, CancellationToken cancellationToken)
    {
        var accountId = new PersistenceAccountId(account.AccountId.Value);
        var syncRules = await syncRuleRepository.GetByAccountAsync(accountId, cancellationToken).ConfigureAwait(false);
        var allSyncedItems = await syncedItemRepository.GetByAccountAsync(accountId, cancellationToken).ConfigureAwait(false);
        var syncedItemsMap = allSyncedItems.ToDictionary(item => item.RemotePath, StringComparer.OrdinalIgnoreCase);

        return new Ok<SyncContext, SyncError>(new SyncContext(account, accessToken, syncRules, syncedItemsMap, []));
    }

    private Task<Result<SyncContext, SyncError>> EnumerateRemoteAsync(SyncContext ctx, CancellationToken cancellationToken)
    {
        RaiseSyncProgress(ctx.Account.AccountId.Value, "Enumerating remote folders...", 0, 0, SyncState.Syncing);
        return remoteFolderEnumerator.EnumerateAsync(ctx.Account, ctx.AccessToken, ctx.SyncRules, cancellationToken)
            .MapAsync(remoteItems => ctx with { RemoteItems = remoteItems });
    }

    private Task<Result<SyncContext, SyncError>> DetectRemoteDeletionsAsync(SyncContext ctx, CancellationToken cancellationToken)
    {
        RaiseSyncProgress(ctx.Account.AccountId.Value, "Detecting remote deletions...", 0, 0, SyncState.Syncing);
        return remoteDeletionDetector.DetectAndDeleteAsync(ctx.RemoteItems, ctx.SyncedItemsMap, cancellationToken)
            .MapAsync(_ => ctx);
    }

    private Task<Result<SyncContext, SyncError>> DetectLocalDeletionsAsync(SyncContext ctx, CancellationToken cancellationToken)
    {
        RaiseSyncProgress(ctx.Account.AccountId.Value, "Detecting local deletions...", 0, 0, SyncState.Syncing);
        return localDeletionDetector.DetectAsync(ctx.AccessToken, ctx.Account, ctx.SyncedItemsMap, cancellationToken)
            .MapAsync(_ => ctx);
    }

    private async Task<Result<Unit, SyncError>> ExecuteJobsAsync(SyncContext ctx, CancellationToken cancellationToken)
    {
        var allJobs = BuildAllJobs(ctx);
        if (allJobs.Count > 0)
        {
            RaiseSyncProgress(ctx.Account.AccountId.Value, "Executing jobs...", 0, allJobs.Count, SyncState.Syncing);
            await jobExecutor.ExecuteAsync(allJobs, ctx.AccessToken, ctx.Account.AccountId.Value, ctx.Account.DriveIdValue, ctx.Account.SyncConfig.WorkerCount, RaiseSyncProgress, RaiseJobCompleted, cancellationToken).ConfigureAwait(false);
        }

        await syncRepository.ClearCompletedJobsAsync(new PersistenceAccountId(ctx.Account.AccountId.Value), cancellationToken).ConfigureAwait(false);
        LogSyncCompleted(logger, ctx.Account.AccountId.Value, allJobs.Count);
        RaiseSyncProgress(ctx.Account.AccountId.Value, "Sync complete.", allJobs.Count, allJobs.Count, SyncState.Idle);

        return new Ok<Unit, SyncError>(Unit.Default);
    }

    private List<SyncJob> BuildAllJobs(SyncContext ctx)
    {
        var localSyncPath = ctx.Account.SyncConfig.LocalSyncPath.Value;
        RaiseSyncProgress(ctx.Account.AccountId.Value, "Building download jobs...", 0, 0, SyncState.Syncing);
        var downloadJobs = downloadJobBuilder.Build(ctx.RemoteItems, ctx.SyncedItemsMap, localSyncPath, ctx.Account.AccountId.Value, conflict => ConflictDetected?.Invoke(this, conflict));

        RaiseSyncProgress(ctx.Account.AccountId.Value, "Detecting local changes...", 0, 0, SyncState.Syncing);
        var uploadJobs = localChangeDetector.Detect(localSyncPath, ctx.SyncedItemsMap, "/");

        return [..downloadJobs, ..uploadJobs];
    }

    private void RaiseSyncProgress(SyncProgressEventArgs args) => SyncProgressChanged?.Invoke(this, args);

    private void RaiseSyncProgress(string accountId, string message, int completed, int total, SyncState state)
        => SyncProgressChanged?.Invoke(this, new SyncProgressEventArgs(accountId, message, completed, total, message, state));

    private void RaiseJobCompleted(JobCompletedEventArgs args) => JobCompleted?.Invoke(this, args);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sync started for account {AccountId}")]
    private static partial void LogSyncStarted(ILogger logger, string accountId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sync completed for account {AccountId}, {JobCount} jobs executed")]
    private static partial void LogSyncCompleted(ILogger logger, string accountId, int jobCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Sync failed for account {AccountId}: {ErrorMessage}")]
    private static partial void LogSyncFailed(ILogger logger, string accountId, string errorMessage);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to resolve conflict {ConflictId}: {ErrorMessage}")]
    private static partial void LogResolveFailed(ILogger logger, string conflictId, string errorMessage);
}
