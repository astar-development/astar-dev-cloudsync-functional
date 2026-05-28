using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed partial class SyncWorker(IHttpDownloader downloader, IUploadService uploadService, ILogger<SyncWorker> logger) : ISyncWorker
{
    /// <inheritdoc />
    public Task<Result<Unit, SyncError>> ExecuteAsync(SyncJob job, string accessToken, CancellationToken cancellationToken = default) =>
        job switch
        {
            DownloadJob download => ExecuteDownloadAsync(download, cancellationToken),
            UploadJob upload => ExecuteUploadAsync(upload, accessToken, cancellationToken),
            _ => Task.FromResult<Result<Unit, SyncError>>(new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(Graph.GraphErrorFactory.Unexpected($"Unknown job type: {job.GetType().Name}"))))
        };

    private Task<Result<Unit, SyncError>> ExecuteDownloadAsync(DownloadJob job, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(job.ItemId))
        {
            LogDownloadSkipped(logger, job.RemotePath);

            return Task.FromResult<Result<Unit, SyncError>>(new Ok<Unit, SyncError>(Unit.Default));
        }

        return downloader.DownloadAsync(job.ItemId, job.LocalPath, job.RemoteModified ?? DateTimeOffset.UtcNow, null, cancellationToken);
    }

    private Task<Result<Unit, SyncError>> ExecuteUploadAsync(UploadJob job, string accessToken, CancellationToken cancellationToken) =>
        uploadService.UploadAsync(string.Empty, accessToken, job.LocalPath, job.RemotePath, job.ParentFolderId, cancellationToken)
            .MatchAsync<string, SyncError, Result<Unit, SyncError>>(
                _ => new Ok<Unit, SyncError>(Unit.Default),
                error => new Fail<Unit, SyncError>(error));

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipping download for {RemotePath} — no download URL available")]
    private static partial void LogDownloadSkipped(ILogger logger, string remotePath);
}
