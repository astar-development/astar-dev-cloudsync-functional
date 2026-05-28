using AStar.Dev.FunctionalParadigm;
using PersistenceDriveId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.DriveId;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Executes a single sync job (download or upload).</summary>
public interface ISyncWorker
{
    /// <summary>Executes the given sync job.</summary>
    /// <param name="job">The job to execute.</param>
    /// <param name="accountId">The MSAL HomeAccountId identifier of the account being synced.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="driveId">The strongly-typed OneDrive drive identifier for the account.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> ExecuteAsync(SyncJob job, string accountId, string accessToken, PersistenceDriveId driveId, CancellationToken cancellationToken = default);
}
