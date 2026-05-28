using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Builds download jobs for remote files that are new or have changed since the last sync.</summary>
public sealed class DownloadJobBuilder(IFileSystem fileSystem) : IDownloadJobBuilder
{
    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromSeconds(5);

    /// <summary>Builds the list of download jobs for all file delta items, applying conflict detection and skip rules.</summary>
    /// <param name="remoteItems">The delta items returned by the remote enumeration step.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="localSyncPath">The root local sync path for computing local file paths.</param>
    /// <param name="accountId">The account identifier, used when constructing conflict records.</param>
    /// <param name="onConflict">Callback invoked for each detected conflict.</param>
    /// <returns>The download jobs to execute, excluding skipped and conflicting items.</returns>
    public IReadOnlyList<SyncJob> Build(IReadOnlyList<DeltaItem> remoteItems, Dictionary<string, SyncedItemEntity> syncedItems, string localSyncPath, string accountId, Action<SyncConflict> onConflict)
    {
        var jobs = new List<SyncJob>();

        foreach (var item in remoteItems.OfType<FileDeltaItem>())
        {
            var localPath = BuildLocalPath(localSyncPath, item.RemotePath);
            var localExists = fileSystem.File.Exists(localPath);
            syncedItems.TryGetValue(item.RemotePath, out var tracked);

            var decision = DetermineDecision(item, tracked, localPath, localExists);

            if (decision == DownloadDecision.Download)
            {
                jobs.Add(SyncJobFactory.CreateDownload(item.Id, item.RemotePath, localPath, item.ETag, item.LastModified));
            }
            else if (decision == DownloadDecision.Conflict)
            {
                var localModified = localExists ? fileSystem.File.GetLastWriteTimeUtc(localPath) : DateTime.MinValue;
                var remoteModified = item.LastModified?.UtcDateTime ?? DateTime.MinValue;
                onConflict(SyncConflictFactory.CreatePending(new SyncConflictId(Guid.NewGuid().ToString()), new AccountId(accountId), new OneDriveItemId(item.Id), new DateTimeOffset(localModified), new DateTimeOffset(remoteModified)));
            }
        }

        return jobs;
    }

    private DownloadDecision DetermineDecision(FileDeltaItem item, SyncedItemEntity? tracked, string localPath, bool localExists)
    {
        if (item.ETag is not null && tracked?.ETag == item.ETag && localExists)
        {
            var localModified = fileSystem.File.GetLastWriteTimeUtc(localPath);
            var remoteModified = item.LastModified?.UtcDateTime ?? DateTime.MinValue;
            var withinTolerance = Math.Abs((localModified - remoteModified).TotalSeconds) <= TimestampTolerance.TotalSeconds;

            return withinTolerance ? DownloadDecision.Skip : DownloadDecision.Conflict;
        }

        if (!localExists)
            return DownloadDecision.Download;

        if (tracked is not null)
        {
            var localModified = fileSystem.File.GetLastWriteTimeUtc(localPath);
            if (localModified > tracked.RemoteModifiedAt.UtcDateTime.Add(TimestampTolerance))
                return DownloadDecision.Conflict;

            return DownloadDecision.Download;
        }

        return DownloadDecision.Conflict;
    }

    private string BuildLocalPath(string localSyncPath, string remotePath)
    {
        var normalised = remotePath.TrimStart('/').Replace('/', fileSystem.Path.DirectorySeparatorChar);

        return fileSystem.Path.Combine(localSyncPath, normalised);
    }

    private enum DownloadDecision { Download, Skip, Conflict }
}
