using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
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

        var upsertResult = await syncedItemRepository.UpsertAsync(entity, cancellationToken).ConfigureAwait(false);
        upsertResult.Match(
            _ => { syncedItems[remotePath] = entity; return Unit.Default; },
            error => { LogUpsertFailed(logger, remotePath, error.Message); return Unit.Default; });
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Created local directory {LocalPath}")]
    private static partial void LogDirectoryCreated(ILogger logger, string localPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to upsert synced item for {RemotePath}: {ErrorMessage}")]
    private static partial void LogUpsertFailed(ILogger logger, string remotePath, string errorMessage);
}
