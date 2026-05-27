using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for <see cref="SyncedItemEntity"/> operations.</summary>
public interface ISyncedItemRepository
{
    /// <summary>Retrieves a synced item by its identifier.</summary>
    /// <param name="id">The synced item identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The item if found, otherwise None.</returns>
    Task<Option<SyncedItemEntity, PersistenceError>> GetByIdAsync(SyncedItemId id, CancellationToken ct = default);

    /// <summary>Retrieves all synced items for a given account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All synced items for the account.</returns>
    Task<IReadOnlyList<SyncedItemEntity>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default);

    /// <summary>Upserts a synced item.</summary>
    /// <param name="entity">The item to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertAsync(SyncedItemEntity entity, CancellationToken ct = default);

    /// <summary>Deletes a synced item by identifier.</summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> DeleteAsync(SyncedItemId id, CancellationToken ct = default);
}
