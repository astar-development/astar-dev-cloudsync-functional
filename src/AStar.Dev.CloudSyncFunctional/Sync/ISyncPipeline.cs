namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Executes a set of sync jobs in parallel using a bounded channel.</summary>
public interface ISyncPipeline
{
    /// <summary>Runs all given jobs using the specified number of parallel workers.</summary>
    /// <param name="jobs">The sync jobs to execute.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="onProgress">Callback invoked after each job completes.</param>
    /// <param name="onJobCompleted">Callback invoked with the result of each individual job.</param>
    /// <param name="accountId">The account identifier the jobs belong to.</param>
    /// <param name="workerCount">The number of parallel workers to use (1–10).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when all jobs have been processed.</returns>
    Task RunAsync(IEnumerable<SyncJob> jobs, string accessToken, Action<SyncProgressEventArgs> onProgress, Action<JobCompletedEventArgs> onJobCompleted, string accountId, int workerCount, CancellationToken cancellationToken = default);
}
