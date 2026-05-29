using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for sync rule operations.</summary>
public interface ISyncRuleRepository
{
    /// <summary>Retrieves all sync rules for a given account as domain records.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All sync rules for the account.</returns>
    Task<IReadOnlyList<SyncRule>> GetByAccountAsync(AccountId accountId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves sync rules for multiple accounts in a single query, keyed by account identifier.</summary>
    /// <param name="accountIds">The account identifiers to load rules for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary mapping each account identifier to its sync rules.</returns>
    Task<IReadOnlyDictionary<AccountId, IReadOnlyList<SyncRule>>> GetAllByAccountIdsAsync(IEnumerable<AccountId> accountIds, CancellationToken cancellationToken = default);

    /// <summary>Upserts a sync rule.</summary>
    /// <param name="entity">The sync rule entity to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertAsync(SyncRuleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sync rule by identifier.</summary>
    /// <param name="id">The sync rule identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> DeleteAsync(SyncRuleId id, CancellationToken cancellationToken = default);
}
