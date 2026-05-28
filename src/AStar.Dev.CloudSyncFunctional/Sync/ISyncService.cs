using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Drives the sync pipeline for OneDrive accounts.</summary>
public interface ISyncService
{
    /// <summary>Raised when sync progress changes for any running account.</summary>
    event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;

    /// <summary>Raised when an individual sync job (download or upload) completes.</summary>
    event EventHandler<JobCompletedEventArgs>? JobCompleted;

    /// <summary>Raised when a conflict is detected during a sync pass.</summary>
    event EventHandler<SyncConflict>? ConflictDetected;

    /// <summary>Runs a full sync pass for the given account.</summary>
    /// <param name="account">The account to sync.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> SyncAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default);

    /// <summary>Applies a conflict resolution policy to a previously detected conflict.</summary>
    /// <param name="conflict">The conflict to resolve.</param>
    /// <param name="policy">The resolution policy to apply.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<Unit, SyncError>> ResolveConflictAsync(SyncConflict conflict, ConflictPolicy policy, CancellationToken cancellationToken = default);
}
