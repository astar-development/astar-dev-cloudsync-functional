using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using PersistenceDriveId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.DriveId;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed partial class SyncWorker(IGraphService graphService, IHttpDownloader downloader, IUploadService uploadService, ILogger<SyncWorker> logger) : ISyncWorker
{
    /// <inheritdoc />
    public Task<Result<Unit, SyncError>> ExecuteAsync(SyncJob job, string accountId, string accessToken, PersistenceDriveId driveId, CancellationToken cancellationToken = default) =>
        job switch
        {
            DownloadJob download => ExecuteDownloadAsync(download, accountId, accessToken, cancellationToken),
            UploadJob upload => ExecuteUploadAsync(upload, accountId, accessToken, driveId, cancellationToken),
            _ => Task.FromResult<Result<Unit, SyncError>>(new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(GraphErrorFactory.Unexpected($"Unknown job type: {job.GetType().Name}"))))
        };

    private Task<Result<Unit, SyncError>> ExecuteDownloadAsync(DownloadJob job, string accountId, string accessToken, CancellationToken cancellationToken) =>
        graphService.GetDownloadUrlAsync(accountId, accessToken, job.ItemId, cancellationToken)
            .MatchAsync(
                url => downloader.DownloadAsync(url, job.LocalPath, job.RemoteModified ?? DateTimeOffset.UtcNow, null, cancellationToken),
                error =>
                {
                    LogDownloadUrlFailed(logger, job.RemotePath, error.Message);

                    return Task.FromResult<Result<Unit, SyncError>>(new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(error)));
                });

    private Task<Result<Unit, SyncError>> ExecuteUploadAsync(UploadJob job, string accountId, string accessToken, PersistenceDriveId driveId, CancellationToken cancellationToken) =>
        graphService.GetFolderIdByPathAsync(accessToken, driveId, job.ParentFolderPath.TrimStart('/'), cancellationToken)
            .MatchAsync(
                folderId => uploadService.UploadAsync(accountId, accessToken, job.LocalPath, job.RemotePath, folderId, cancellationToken)
                    .MatchAsync(
                        _ => Task.FromResult<Result<Unit, SyncError>>(new Ok<Unit, SyncError>(Unit.Default)),
                        error =>
                        {
                            LogUploadFailed(logger, job.RemotePath, error.Message);

                            return Task.FromResult<Result<Unit, SyncError>>(new Fail<Unit, SyncError>(error));
                        }),
                _ =>
                {
                    LogParentFolderNotFound(logger, job.ParentFolderPath);

                    return Task.FromResult<Result<Unit, SyncError>>(new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(GraphErrorFactory.NotFound(job.ParentFolderPath))));
                });

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to resolve download URL for {RemotePath}: {ErrorMessage}")]
    private static partial void LogDownloadUrlFailed(ILogger logger, string remotePath, string errorMessage);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Upload failed for {RemotePath}: {ErrorMessage}")]
    private static partial void LogUploadFailed(ILogger logger, string remotePath, string errorMessage);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Parent folder not found in OneDrive for path {ParentFolderPath}")]
    private static partial void LogParentFolderNotFound(ILogger logger, string parentFolderPath);
}
