using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Downloads files from pre-signed URLs to the local file system.</summary>
public interface IHttpDownloader
{
    /// <summary>Downloads the file at the given URL and writes it to the local path, then sets the file timestamps to match the remote modification time.</summary>
    /// <param name="url">The pre-signed download URL.</param>
    /// <param name="localPath">The local destination path.</param>
    /// <param name="remoteModified">The remote last-modified timestamp; written to the local file after download.</param>
    /// <param name="progress">Optional progress reporter that receives the number of bytes written so far.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> DownloadAsync(string url, string localPath, DateTimeOffset remoteModified, IProgress<long>? progress = null, CancellationToken cancellationToken = default);
}
