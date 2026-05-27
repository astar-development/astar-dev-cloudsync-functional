using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for <see cref="DriveStateEntity"/> operations.</summary>
public interface IDriveStateRepository
{
    /// <summary>Retrieves the drive state for a given account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The drive state if present, otherwise None.</returns>
    Task<Option<DriveStateEntity, PersistenceError>> GetByAccountAsync(AccountId accountId, CancellationToken cancellationToken = default);

    /// <summary>Upserts the drive state for an account.</summary>
    /// <param name="entity">The drive state to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertAsync(DriveStateEntity entity, CancellationToken cancellationToken = default);
}
