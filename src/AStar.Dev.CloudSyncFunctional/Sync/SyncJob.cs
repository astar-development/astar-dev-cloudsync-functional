namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Base type for sync jobs produced by the pipeline build phase.</summary>
public abstract record SyncJob;

/// <summary>A job to download a remote file to the local file system.</summary>
/// <param name="ItemId">The Graph item identifier of the remote file.</param>
/// <param name="RemotePath">The remote OneDrive path of the file.</param>
/// <param name="LocalPath">The local destination path.</param>
/// <param name="ETag">The remote eTag at the time the job was created, if known.</param>
/// <param name="RemoteModified">The remote last-modified timestamp, if known.</param>
public sealed record DownloadJob(string ItemId, string RemotePath, string LocalPath, string? ETag, DateTimeOffset? RemoteModified) : SyncJob;

/// <summary>A job to upload a local file to OneDrive.</summary>
/// <param name="LocalPath">The local source path of the file to upload.</param>
/// <param name="RemotePath">The destination remote OneDrive path.</param>
/// <param name="ParentFolderId">The Graph item identifier of the destination parent folder.</param>
public sealed record UploadJob(string LocalPath, string RemotePath, string ParentFolderId) : SyncJob;

/// <summary>Creates <see cref="SyncJob"/> instances.</summary>
public static class SyncJobFactory
{
    /// <summary>Creates a <see cref="DownloadJob"/>.</summary>
    /// <param name="itemId">The Graph item identifier of the remote file.</param>
    /// <param name="remotePath">The remote OneDrive path.</param>
    /// <param name="localPath">The local destination path.</param>
    /// <param name="eTag">The remote eTag, if known.</param>
    /// <param name="remoteModified">The remote last-modified timestamp, if known.</param>
    /// <returns>A new <see cref="DownloadJob"/>.</returns>
    public static SyncJob CreateDownload(string itemId, string remotePath, string localPath, string? eTag, DateTimeOffset? remoteModified) =>
        new DownloadJob(itemId, remotePath, localPath, eTag, remoteModified);

    /// <summary>Creates an <see cref="UploadJob"/>.</summary>
    /// <param name="localPath">The local source path.</param>
    /// <param name="remotePath">The destination remote OneDrive path.</param>
    /// <param name="parentFolderId">The Graph item identifier of the destination parent folder.</param>
    /// <returns>A new <see cref="UploadJob"/>.</returns>
    public static SyncJob CreateUpload(string localPath, string remotePath, string parentFolderId) =>
        new UploadJob(localPath, remotePath, parentFolderId);
}
