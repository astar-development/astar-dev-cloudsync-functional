using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class FileClassificationRuleRepository(IDbContextFactory<AppDbContext> dbFactory) : IFileClassificationRuleRepository
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<FileClassificationRuleEntity>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertAsync(FileClassificationRuleEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> DeleteAsync(string id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}
