using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for sync conflict and job operations.</summary>
public interface ISyncRepository
{
    /// <summary>Retrieves all pending conflicts for a given account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All pending conflicts for the account.</returns>
    Task<IReadOnlyList<SyncConflictEntity>> GetPendingConflictsAsync(AccountId accountId, CancellationToken cancellationToken = default);

    /// <summary>Upserts a sync conflict.</summary>
    /// <param name="entity">The conflict to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertConflictAsync(SyncConflictEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Marks a conflict as resolved.</summary>
    /// <param name="id">The conflict identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> ResolveConflictAsync(SyncConflictId id, CancellationToken cancellationToken = default);

    /// <summary>Upserts a sync job.</summary>
    /// <param name="entity">The job to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertJobAsync(SyncJobEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Removes all completed jobs for a given account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> ClearCompletedJobsAsync(AccountId accountId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all jobs in "Running" state for a given account (crash survivors).</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Jobs with Status == "Running".</returns>
    Task<IReadOnlyList<SyncJobEntity>> GetInterruptedJobsAsync(AccountId accountId, CancellationToken cancellationToken = default);

    /// <summary>Resets all "Running" jobs for an account to "Interrupted" status.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> ResetInterruptedJobsAsync(AccountId accountId, CancellationToken cancellationToken = default);
}
