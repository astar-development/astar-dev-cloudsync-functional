using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Detects and removes local files whose remote counterparts have been deleted.</summary>
public sealed partial class RemoteDeletionDetector(ISyncedItemRepository syncedItemRepository, IFileSystem fileSystem, ILogger<RemoteDeletionDetector> logger) : IRemoteDeletionDetector
{
    /// <summary>Detects items present in the local tracking store but absent from the remote enumeration result, and deletes those local files.</summary>
    /// <param name="remoteItems">The items currently present in the remote drive.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    public async Task<Result<Unit, SyncError>> DetectAndDeleteAsync(IReadOnlyList<DeltaItem> remoteItems, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken)
    {
        var remotePaths = remoteItems.Select(GetRemotePath).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (remotePath, tracked) in syncedItems.Where(kvp => !remotePaths.Contains(kvp.Key)).ToList())
        {
            if (!tracked.IsFolder && fileSystem.File.Exists(tracked.LocalPath))
            {
                fileSystem.File.Delete(tracked.LocalPath);
                LogLocalFileDeleted(logger, tracked.LocalPath);
            }

            var deleteError = await syncedItemRepository.DeleteAsync(tracked.Id, cancellationToken)
                .MatchAsync<Unit, Onboarding.PersistenceError, Onboarding.PersistenceError?>(
                    _ => (Onboarding.PersistenceError?)null,
                    error =>
                    {
                        LogTrackingDeleteFailed(logger, remotePath, error.Message);

                        return (Onboarding.PersistenceError?)error;
                    });

            if (deleteError is not null)
                return new Fail<Unit, SyncError>(SyncErrorFactory.StorageFailed(deleteError));

            syncedItems.Remove(remotePath);
        }

        return new Ok<Unit, SyncError>(Unit.Default);
    }

    private static string GetRemotePath(DeltaItem item) =>
        item switch
        {
            FileDeltaItem file => file.RemotePath,
            FolderDeltaItem folder => folder.RemotePath,
            DeletedDeltaItem deleted => deleted.RemotePath,
            _ => string.Empty
        };

    [LoggerMessage(Level = LogLevel.Debug, Message = "Deleted local file {LocalPath} — remote was removed")]
    private static partial void LogLocalFileDeleted(ILogger logger, string localPath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to delete tracking record for {RemotePath}: {ErrorMessage}")]
    private static partial void LogTrackingDeleteFailed(ILogger logger, string remotePath, string errorMessage);
}
