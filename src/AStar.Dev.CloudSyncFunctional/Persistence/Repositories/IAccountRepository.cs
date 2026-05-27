using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for <see cref="AccountEntity"/> operations.</summary>
public interface IAccountRepository
{
    /// <summary>Retrieves an account by its identifier.</summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The account if found, otherwise None.</returns>
    Task<Option<AccountEntity, PersistenceError>> GetByIdAsync(AccountId id, CancellationToken ct = default);

    /// <summary>Retrieves all accounts.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All stored accounts.</returns>
    Task<IReadOnlyList<AccountEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Upserts an account.</summary>
    /// <param name="entity">The account to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertAsync(AccountEntity entity, CancellationToken ct = default);

    /// <summary>Deletes an account and all its child entities.</summary>
    /// <param name="id">The account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> DeleteAsync(AccountId id, CancellationToken ct = default);
}
