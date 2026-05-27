using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class SyncRepository(IDbContextFactory<AppDbContext> dbFactory) : ISyncRepository
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<SyncConflictEntity>> GetPendingConflictsAsync(AccountId accountId, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertConflictAsync(SyncConflictEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> ResolveConflictAsync(SyncConflictId id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertJobAsync(SyncJobEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> ClearCompletedJobsAsync(AccountId accountId, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}
