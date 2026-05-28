using AStar.Dev.CloudSyncFunctional.Graph;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed class SyncWorkerFactory(IGraphService graphService, IHttpDownloader downloader, IUploadService uploadService, ILoggerFactory loggerFactory) : ISyncWorkerFactory
{
    /// <inheritdoc />
    public ISyncWorker Create() => new SyncWorker(graphService, downloader, uploadService, loggerFactory.CreateLogger<SyncWorker>());
}
