namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Base type for items returned by the remote folder enumeration step.</summary>
public abstract record DeltaItem;

/// <summary>A remote file item.</summary>
/// <param name="Id">The Graph item identifier.</param>
/// <param name="Name">The display name of the file.</param>
/// <param name="RemotePath">The full remote OneDrive path.</param>
/// <param name="ETag">The remote eTag, if available.</param>
/// <param name="LastModified">The remote last-modified timestamp, if available.</param>
public sealed record FileDeltaItem(string Id, string Name, string RemotePath, string? ETag, DateTimeOffset? LastModified) : DeltaItem;

/// <summary>A remote folder item.</summary>
/// <param name="Id">The Graph item identifier.</param>
/// <param name="Name">The display name of the folder.</param>
/// <param name="RemotePath">The full remote OneDrive path.</param>
public sealed record FolderDeltaItem(string Id, string Name, string RemotePath) : DeltaItem;

/// <summary>An item that has been deleted from the remote drive.</summary>
/// <param name="Id">The Graph item identifier of the deleted item.</param>
/// <param name="RemotePath">The remote OneDrive path the item occupied before deletion.</param>
public sealed record DeletedDeltaItem(string Id, string RemotePath) : DeltaItem;
