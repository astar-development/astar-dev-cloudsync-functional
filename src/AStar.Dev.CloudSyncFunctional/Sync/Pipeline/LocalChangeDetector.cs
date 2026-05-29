using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Detects locally changed or new files and builds upload jobs for them.</summary>
public sealed class LocalChangeDetector(IFileSystem fileSystem) : ILocalChangeDetector
{
    private static readonly TimeSpan TimestampTolerance = TimeSpan.FromSeconds(5);
    private static readonly HashSet<string> SkippedExtensions = [".tmp", ".temp", ".partial"];

    /// <summary>Walks the local sync directory tree and builds upload jobs for files that are new or modified.</summary>
    /// <param name="localSyncPath">The root local sync path to walk.</param>
    /// <param name="syncedItems">The in-memory tracking dictionary keyed on remote path.</param>
    /// <param name="remoteSyncPath">The remote base path used when computing remote paths for upload jobs.</param>
    /// <returns>Upload jobs for all new and modified local files.</returns>
    public IReadOnlyList<SyncJob> Detect(string localSyncPath, Dictionary<string, SyncedItemEntity> syncedItems, string remoteSyncPath)
    {
        if (!fileSystem.Directory.Exists(localSyncPath))
            return [];

        var jobs = new List<SyncJob>();
        WalkDirectory(localSyncPath, localSyncPath, remoteSyncPath, syncedItems, jobs);

        return jobs;
    }

    private void WalkDirectory(string directory, string localSyncRoot, string remoteSyncRoot, Dictionary<string, SyncedItemEntity> syncedItems, List<SyncJob> jobs)
    {
        foreach (var subDir in fileSystem.Directory.GetDirectories(directory))
        {
            var dirName = fileSystem.Path.GetFileName(subDir);
            if (IsHiddenOrDot(dirName))
                continue;

            WalkDirectory(subDir, localSyncRoot, remoteSyncRoot, syncedItems, jobs);
        }

        foreach (var file in fileSystem.Directory.GetFiles(directory))
        {
            if (ShouldSkip(file))
                continue;

            var relativePath = GetRelativePath(localSyncRoot, file);
            var remotePath = $"{remoteSyncRoot.TrimEnd('/')}/{relativePath.Replace(fileSystem.Path.DirectorySeparatorChar, '/')}";

            if (!NeedsUpload(file, remotePath, syncedItems))
                continue;

            var parentRemotePath = fileSystem.Path.GetDirectoryName(remotePath.Replace('/', fileSystem.Path.DirectorySeparatorChar))?.Replace(fileSystem.Path.DirectorySeparatorChar, '/') ?? remoteSyncRoot;
            jobs.Add(SyncJobFactory.CreateUpload(file, remotePath, parentRemotePath));
        }
    }

    private bool ShouldSkip(string filePath)
    {
        var fileName = fileSystem.Path.GetFileName(filePath);
        if (IsHiddenOrDot(fileName))
            return true;

        var extension = fileSystem.Path.GetExtension(filePath);

        return SkippedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private bool NeedsUpload(string localPath, string remotePath, Dictionary<string, SyncedItemEntity> syncedItems)
    {
        if (!syncedItems.TryGetValue(remotePath, out var tracked))
            return true;

        var localModified = fileSystem.File.GetLastWriteTimeUtc(localPath);

        return localModified > tracked.RemoteModifiedAt.UtcDateTime.Add(TimestampTolerance);
    }

    private string GetRelativePath(string basePath, string fullPath)
    {
        var normalised = fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)
            ? fullPath[basePath.Length..].TrimStart(fileSystem.Path.DirectorySeparatorChar)
            : fullPath;

        return normalised;
    }

    private static bool IsHiddenOrDot(string name) => name.StartsWith('.') || name.StartsWith('~');
}
