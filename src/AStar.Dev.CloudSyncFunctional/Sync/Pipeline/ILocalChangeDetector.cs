using AStar.Dev.CloudSyncFunctional.Persistence.Entities;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Detects locally changed or new files and builds upload jobs for them.</summary>
public interface ILocalChangeDetector
{
    /// <summary>Walks the local sync directory tree and builds upload jobs for files that are new or modified.</summary>
    /// <param name="localSyncPath">The root local sync path to walk.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="remoteSyncPath">The remote base path used when computing remote paths for upload jobs.</param>
    /// <returns>Upload jobs for all new and modified local files.</returns>
    IReadOnlyList<SyncJob> Detect(string localSyncPath, Dictionary<string, SyncedItemEntity> syncedItems, string remoteSyncPath);
}
