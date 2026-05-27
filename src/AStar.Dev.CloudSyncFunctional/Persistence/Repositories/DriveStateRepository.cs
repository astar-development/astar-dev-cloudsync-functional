using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class DriveStateRepository(IDbContextFactory<AppDbContext> dbFactory) : IDriveStateRepository
{
    /// <inheritdoc/>
    public Task<Option<DriveStateEntity, PersistenceError>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertAsync(DriveStateEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}
