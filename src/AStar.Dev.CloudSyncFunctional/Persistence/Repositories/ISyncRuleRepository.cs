using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for <see cref="SyncRuleEntity"/> operations.</summary>
public interface ISyncRuleRepository
{
    /// <summary>Retrieves all sync rules for a given account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All sync rules for the account.</returns>
    Task<IReadOnlyList<SyncRuleEntity>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default);

    /// <summary>Upserts a sync rule.</summary>
    /// <param name="entity">The sync rule to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertAsync(SyncRuleEntity entity, CancellationToken ct = default);

    /// <summary>Deletes a sync rule by identifier.</summary>
    /// <param name="id">The sync rule identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> DeleteAsync(SyncRuleId id, CancellationToken ct = default);
}
