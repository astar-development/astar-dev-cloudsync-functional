using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed class SyncWorkerFactory(IHttpDownloader downloader, IUploadService uploadService, ILoggerFactory loggerFactory) : ISyncWorkerFactory
{
    /// <inheritdoc />
    public ISyncWorker Create() => new SyncWorker(downloader, uploadService, loggerFactory.CreateLogger<SyncWorker>());
}
