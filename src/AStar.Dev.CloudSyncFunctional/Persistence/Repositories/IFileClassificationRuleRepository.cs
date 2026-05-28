using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <summary>Persistence contract for <see cref="FileClassificationRuleEntity"/> operations.</summary>
public interface IFileClassificationRuleRepository
{
    /// <summary>Retrieves all file classification rules.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All stored classification rules.</returns>
    Task<IReadOnlyList<FileClassificationRuleEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Upserts a file classification rule.</summary>
    /// <param name="entity">The rule to upsert.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> UpsertAsync(FileClassificationRuleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes a file classification rule by identifier.</summary>
    /// <param name="id">The rule identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ok on success, Fail on error.</returns>
    Task<Result<Unit, PersistenceError>> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
