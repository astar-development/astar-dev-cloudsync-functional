using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Registers discovered folders in the local file system and in the sync tracking store.</summary>
public interface ISyncedItemRegistrar
{
    /// <summary>Creates the local directory for the given folder item if it does not exist, and upserts the tracking record.</summary>
    /// <param name="accountId">The account this folder belongs to.</param>
    /// <param name="item">The folder delta item discovered during enumeration.</param>
    /// <param name="remotePath">The full remote OneDrive path of the folder.</param>
    /// <param name="localPath">The local file system path where the folder should be created.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the folder is registered.</returns>
    Task RegisterFolderAsync(AccountId accountId, FolderDeltaItem item, string remotePath, string localPath, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken = default);
}
