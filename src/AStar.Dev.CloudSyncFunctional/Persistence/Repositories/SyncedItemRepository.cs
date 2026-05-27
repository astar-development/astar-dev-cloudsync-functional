using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class SyncedItemRepository(IDbContextFactory<AppDbContext> dbFactory) : ISyncedItemRepository
{
    /// <inheritdoc/>
    public Task<Option<SyncedItemEntity, PersistenceError>> GetByIdAsync(SyncedItemId id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<IReadOnlyList<SyncedItemEntity>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertAsync(SyncedItemEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> DeleteAsync(SyncedItemId id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}
