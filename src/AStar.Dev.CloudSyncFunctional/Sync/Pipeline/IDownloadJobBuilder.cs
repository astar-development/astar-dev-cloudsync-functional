using AStar.Dev.CloudSyncFunctional.Persistence.Entities;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Builds download jobs for remote files that are new or have changed since the last sync.</summary>
public interface IDownloadJobBuilder
{
    /// <summary>Builds the list of download jobs for all file delta items, applying conflict detection and skip rules.</summary>
    /// <param name="remoteItems">The delta items returned by the remote enumeration step.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="localSyncPath">The root local sync path for computing local file paths.</param>
    /// <param name="accountId">The account identifier, used when constructing conflict records.</param>
    /// <param name="onConflict">Callback invoked for each detected conflict.</param>
    /// <returns>The download jobs to execute, excluding skipped and conflicting items.</returns>
    IReadOnlyList<SyncJob> Build(IReadOnlyList<DeltaItem> remoteItems, Dictionary<string, SyncedItemEntity> syncedItems, string localSyncPath, string accountId, Action<SyncConflict> onConflict);
}
