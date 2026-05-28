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
    /// <inheritdoc />
    public event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;

    /// <inheritdoc />
    public event EventHandler<JobCompletedEventArgs>? JobCompleted;

    /// <inheritdoc />
    public event EventHandler<SyncConflict>? ConflictDetected;

    /// <inheritdoc />
    public async Task<Result<Unit, SyncError>> SyncAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default)
    {
        LogSyncStarted(logger, account.AccountId.Value);
        RaiseSyncProgress(account.AccountId.Value, "Acquiring token...", 0, 0, SyncState.Syncing);

        string? accessToken = null;
        SyncError? tokenError = null;
        await authService.AcquireTokenSilentAsync(account.AccountId.Value, cancellationToken)
            .MatchAsync(
                authResult => { accessToken = authResult.AccessToken; },
                error =>
                {
                    LogSyncFailed(logger, account.AccountId.Value, error.Message);
                    tokenError = SyncErrorFactory.AuthFailed(error);
                });
        if (tokenError is not null)
            return new Fail<Unit, SyncError>(tokenError);

        var accountId = new PersistenceAccountId(account.AccountId.Value);
        var entityRules = await syncRuleRepository.GetByAccountAsync(accountId, cancellationToken).ConfigureAwait(false);
        var syncRules = entityRules.Select(r => r.RuleType == RuleType.Include
            ? SyncRuleFactory.CreateInclude(r.RemotePath)
            : SyncRuleFactory.CreateExclude(r.RemotePath)).ToList();
        var allSyncedItems = await syncedItemRepository.GetByAccountAsync(accountId, cancellationToken).ConfigureAwait(false);
        var syncedItemsMap = allSyncedItems.ToDictionary(item => item.RemotePath, StringComparer.OrdinalIgnoreCase);

        RaiseSyncProgress(account.AccountId.Value, "Enumerating remote folders...", 0, 0, SyncState.Syncing);
        SyncError? enumerateError = null;
        List<DeltaItem> remoteItems = [];
        await remoteFolderEnumerator.EnumerateAsync(account, accessToken!, syncRules, cancellationToken)
            .MatchAsync(
                items => remoteItems = items,
                error =>
                {
                    LogSyncFailed(logger, account.AccountId.Value, error.Message);
                    enumerateError = error;
                });
        if (enumerateError is not null)
            return new Fail<Unit, SyncError>(enumerateError);

        RaiseSyncProgress(account.AccountId.Value, "Detecting remote deletions...", 0, 0, SyncState.Syncing);
        var remoteDeletionError = await remoteDeletionDetector.DetectAndDeleteAsync(remoteItems, syncedItemsMap, cancellationToken)
            .MatchAsync<Unit, SyncError, SyncError?>(_ => null, error => error);
        if (remoteDeletionError is not null)
            return new Fail<Unit, SyncError>(remoteDeletionError);

        RaiseSyncProgress(account.AccountId.Value, "Detecting local deletions...", 0, 0, SyncState.Syncing);
        var localDeletionError = await localDeletionDetector.DetectAsync(accessToken!, account, syncedItemsMap, cancellationToken)
            .MatchAsync<Unit, SyncError, SyncError?>(_ => null, error => error);
        if (localDeletionError is not null)
            return new Fail<Unit, SyncError>(localDeletionError);

        var localSyncPath = account.SyncConfig.LocalSyncPath.Value;
        RaiseSyncProgress(account.AccountId.Value, "Building download jobs...", 0, 0, SyncState.Syncing);
        var downloadJobs = downloadJobBuilder.Build(remoteItems, syncedItemsMap, localSyncPath, account.AccountId.Value, conflict => ConflictDetected?.Invoke(this, conflict));

        RaiseSyncProgress(account.AccountId.Value, "Detecting local changes...", 0, 0, SyncState.Syncing);
        var uploadJobs = localChangeDetector.Detect(localSyncPath, syncedItemsMap, "/");

        var allJobs = downloadJobs.Concat(uploadJobs).ToList();
        if (allJobs.Count > 0)
        {
            RaiseSyncProgress(account.AccountId.Value, "Executing jobs...", 0, allJobs.Count, SyncState.Syncing);
            await jobExecutor.ExecuteAsync(allJobs, accessToken!, account.AccountId.Value, account.DriveIdValue, account.SyncConfig.WorkerCount, RaiseSyncProgress, RaiseJobCompleted, cancellationToken).ConfigureAwait(false);
        }

        await syncRepository.ClearCompletedJobsAsync(accountId, cancellationToken).ConfigureAwait(false);

        LogSyncCompleted(logger, account.AccountId.Value, allJobs.Count);
        RaiseSyncProgress(account.AccountId.Value, "Sync complete.", allJobs.Count, allJobs.Count, SyncState.Idle);

        return new Ok<Unit, SyncError>(Unit.Default);
    }

    /// <inheritdoc />
    public async Task<Result<Unit, SyncError>> ResolveConflictAsync(SyncConflict conflict, ConflictPolicy policy, CancellationToken cancellationToken = default)
    {
        var resolveError = await syncRepository.ResolveConflictAsync(new Persistence.ValueObjects.SyncConflictId(conflict.Id.Value), cancellationToken)
            .MatchAsync<Unit, Onboarding.PersistenceError, SyncError?>(
                _ => (SyncError?)null,
                error =>
                {
                    LogResolveFailed(logger, conflict.Id.Value, error.Message);

                    return (SyncError?)SyncErrorFactory.StorageFailed(error);
                });

        if (resolveError is not null)
            return new Fail<Unit, SyncError>(resolveError);

        return new Ok<Unit, SyncError>(Unit.Default);
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
