using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Executes a single sync job (download or upload).</summary>
public interface ISyncWorker
{
    /// <summary>Executes the given sync job.</summary>
    /// <param name="job">The job to execute.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> ExecuteAsync(SyncJob job, string accessToken, CancellationToken cancellationToken = default);
}
