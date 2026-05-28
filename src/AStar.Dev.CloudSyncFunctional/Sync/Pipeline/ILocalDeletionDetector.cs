using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Detects local file deletions and removes the corresponding remote items from OneDrive.</summary>
public interface ILocalDeletionDetector
{
    /// <summary>Scans tracked items for local files that no longer exist, then deletes those items from OneDrive and removes their tracking records.</summary>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="account">The account being synced.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> DetectAsync(string accessToken, OneDriveAccount account, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken);
}
