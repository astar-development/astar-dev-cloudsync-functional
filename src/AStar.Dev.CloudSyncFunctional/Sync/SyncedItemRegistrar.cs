using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed partial class SyncedItemRegistrar(ISyncedItemRepository syncedItemRepository, IFileSystem fileSystem, ILogger<SyncedItemRegistrar> logger) : ISyncedItemRegistrar
{
    /// <inheritdoc />
    public async Task RegisterFolderAsync(AccountId accountId, FolderDeltaItem item, string remotePath, string localPath, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken = default)
    {
        if (!fileSystem.Directory.Exists(localPath))
        {
            fileSystem.Directory.CreateDirectory(localPath);
            LogDirectoryCreated(logger, localPath);
        }

        var entity = new SyncedItemEntity
        {
            Id = new SyncedItemId(Guid.NewGuid().ToString()),
            AccountId = accountId,
            RemotePath = remotePath,
            LocalPath = localPath,
            RemoteModifiedAt = DateTimeOffset.UtcNow,
            IsFolder = true
        };

        await syncedItemRepository.UpsertAsync(entity, cancellationToken).ConfigureAwait(false);
        syncedItems[remotePath] = entity;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Created local directory {LocalPath}")]
    private static partial void LogDirectoryCreated(ILogger logger, string localPath);
}
