using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Detects local file deletions and removes the corresponding remote items from OneDrive.</summary>
public sealed partial class LocalDeletionDetector(IGraphService graphService, ISyncedItemRepository syncedItemRepository, IFileSystem fileSystem, ILogger<LocalDeletionDetector> logger)
{
    /// <summary>Scans tracked items for local files that no longer exist, then deletes those items from OneDrive and removes their tracking records.</summary>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="account">The account being synced.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    public async Task<Result<Unit, SyncError>> DetectAsync(string accessToken, OneDriveAccount account, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken)
    {
        var locallyDeleted = syncedItems.Values
            .Where(item => !item.IsFolder && !fileSystem.File.Exists(item.LocalPath))
            .ToList();

        foreach (var item in locallyDeleted)
        {
            var remoteDeleteError = await DeleteRemoteItemIfFoundAsync(accessToken, account, item.RemotePath, cancellationToken).ConfigureAwait(false);
            if (remoteDeleteError is not null)
                return new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(remoteDeleteError));

            var trackingError = await syncedItemRepository.DeleteAsync(item.Id, cancellationToken)
                .MatchAsync<Unit, Onboarding.PersistenceError, Onboarding.PersistenceError?>(
                    _ => (Onboarding.PersistenceError?)null,
                    error =>
                    {
                        LogTrackingDeleteFailed(logger, item.RemotePath, error.Message);

                        return (Onboarding.PersistenceError?)error;
                    });

            if (trackingError is not null)
                return new Fail<Unit, SyncError>(SyncErrorFactory.StorageFailed(trackingError));

            syncedItems.Remove(item.RemotePath);
            LogLocalDeletionProcessed(logger, item.RemotePath);
        }

        return new Ok<Unit, SyncError>(Unit.Default);
    }

    private async Task<GraphError?> DeleteRemoteItemIfFoundAsync(string accessToken, OneDriveAccount account, string remotePath, CancellationToken cancellationToken)
    {
        var folderId = await graphService.GetFolderIdByPathAsync(accessToken, account.DriveIdValue, remotePath, cancellationToken).ConfigureAwait(false);

        return await folderId.Match<string, Task<GraphError?>>(
            async itemId =>
            {
                var deleteResult = await graphService.DeleteItemAsync(account.AccountId.Value, accessToken, itemId, cancellationToken).ConfigureAwait(false);

                return deleteResult.Match<Unit, GraphError, GraphError?>(
                    _ => null,
                    error =>
                    {
                        LogRemoteDeleteFailed(logger, remotePath, error.Message);

                        return error;
                    });
            },
            _ => Task.FromResult<GraphError?>(null)).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processed local deletion for {RemotePath}")]
    private static partial void LogLocalDeletionProcessed(ILogger logger, string remotePath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Remote delete failed for {RemotePath}: {ErrorMessage}")]
    private static partial void LogRemoteDeleteFailed(ILogger logger, string remotePath, string errorMessage);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to delete tracking record for {RemotePath}: {ErrorMessage}")]
    private static partial void LogTrackingDeleteFailed(ILogger logger, string remotePath, string errorMessage);
}
