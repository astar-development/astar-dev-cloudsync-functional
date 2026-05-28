namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Delegates job execution to <see cref="ISyncPipeline"/>.</summary>
public sealed class JobExecutor(ISyncPipeline syncPipeline)
{
    /// <summary>Executes all given sync jobs using the bounded channel pipeline.</summary>
    /// <param name="jobs">The jobs to execute.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="accountId">The account identifier the jobs belong to.</param>
    /// <param name="workerCount">The number of parallel workers to use.</param>
    /// <param name="onProgress">Callback invoked after each job completes.</param>
    /// <param name="onJobCompleted">Callback invoked with the result of each individual job.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when all jobs have been processed.</returns>
    public Task ExecuteAsync(IEnumerable<SyncJob> jobs, string accessToken, string accountId, int workerCount, Action<SyncProgressEventArgs> onProgress, Action<JobCompletedEventArgs> onJobCompleted, CancellationToken cancellationToken = default)
        => syncPipeline.RunAsync(jobs, accessToken, onProgress, onJobCompleted, accountId, workerCount, cancellationToken);
}
