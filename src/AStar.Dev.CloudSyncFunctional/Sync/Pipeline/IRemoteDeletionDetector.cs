using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Detects and removes local files whose remote counterparts have been deleted.</summary>
public interface IRemoteDeletionDetector
{
    /// <summary>Detects items present in the local tracking store but absent from the remote enumeration result, and deletes those local files.</summary>
    /// <param name="remoteItems">The items currently present in the remote drive.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> DetectAndDeleteAsync(IReadOnlyList<DeltaItem> remoteItems, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken);
}
