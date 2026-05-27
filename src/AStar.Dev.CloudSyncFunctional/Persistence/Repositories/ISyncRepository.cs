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
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All pending conflicts for the account.</returns>
    Task<IReadOnlyList<SyncConflictEntity>> GetPendingConflictsAsync(AccountId accountId, CancellationToken ct = default);

    /// <summary>Upserts a sync conflict.</summary>
    /// <param name="entity">The conflict to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertConflictAsync(SyncConflictEntity entity, CancellationToken ct = default);

    /// <summary>Marks a conflict as resolved.</summary>
    /// <param name="id">The conflict identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> ResolveConflictAsync(SyncConflictId id, CancellationToken ct = default);

    /// <summary>Upserts a sync job.</summary>
    /// <param name="entity">The job to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertJobAsync(SyncJobEntity entity, CancellationToken ct = default);

    /// <summary>Removes all completed jobs for a given account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> ClearCompletedJobsAsync(AccountId accountId, CancellationToken ct = default);
}
