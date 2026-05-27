using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class SyncRuleRepository(IDbContextFactory<AppDbContext> dbFactory) : ISyncRuleRepository
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<SyncRuleEntity>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertAsync(SyncRuleEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> DeleteAsync(SyncRuleId id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}
