using System.Threading.Channels;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed partial class SyncPipeline(ISyncWorkerFactory workerFactory, ILogger<SyncPipeline> logger) : ISyncPipeline
{
    /// <inheritdoc />
    public async Task RunAsync(IEnumerable<SyncJob> jobs, string accessToken, Action<SyncProgressEventArgs> onProgress, Action<JobCompletedEventArgs> onJobCompleted, string accountId, int workerCount, CancellationToken cancellationToken = default)
    {
        var jobList = jobs.ToList();
        var total = jobList.Count;

        var channel = Channel.CreateBounded<SyncJob>(new BoundedChannelOptions(workerCount * 4)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true
        });

        var workerTasks = Enumerable.Range(0, workerCount)
            .Select(_ => RunWorkerAsync(channel.Reader, accessToken, accountId, total, onProgress, onJobCompleted, cancellationToken))
            .ToArray();

        await ProduceJobsAsync(channel.Writer, jobList, cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(workerTasks).ConfigureAwait(false);
    }

    private static async Task ProduceJobsAsync(ChannelWriter<SyncJob> writer, IReadOnlyList<SyncJob> jobs, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var job in jobs)
                await writer.WriteAsync(job, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task RunWorkerAsync(ChannelReader<SyncJob> reader, string accessToken, string accountId, int total, Action<SyncProgressEventArgs> onProgress, Action<JobCompletedEventArgs> onJobCompleted, CancellationToken cancellationToken)
    {
        var worker = workerFactory.Create();
        var localCompleted = 0;

        await foreach (var job in reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            var remotePath = GetRemotePath(job);
            var result = await worker.ExecuteAsync(job, accessToken, cancellationToken).ConfigureAwait(false);

            localCompleted++;
            result.Tap(
                _ => onJobCompleted(new JobCompletedEventArgs(accountId, remotePath, true, null)),
                error =>
                {
                    LogJobFailed(logger, accountId, remotePath, error.Message);
                    onJobCompleted(new JobCompletedEventArgs(accountId, remotePath, false, error.Message));
                });
            onProgress(new SyncProgressEventArgs(accountId, remotePath, localCompleted, total, $"Processed {localCompleted}/{total}", SyncState.Syncing));
        }
    }

    private static string GetRemotePath(SyncJob job) =>
        job switch
        {
            DownloadJob download => download.RemotePath,
            UploadJob upload => upload.RemotePath,
            _ => string.Empty
        };

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sync job failed for account {AccountId}, path {RemotePath}: {ErrorMessage}")]
    private static partial void LogJobFailed(ILogger logger, string accountId, string remotePath, string errorMessage);
}
