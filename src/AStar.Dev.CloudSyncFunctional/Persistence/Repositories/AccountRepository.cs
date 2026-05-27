using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class AccountRepository(IDbContextFactory<AppDbContext> dbFactory) : IAccountRepository
{
    /// <inheritdoc/>
    public Task<Option<AccountEntity, PersistenceError>> GetByIdAsync(AccountId id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<IReadOnlyList<AccountEntity>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> UpsertAsync(AccountEntity entity, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> DeleteAsync(AccountId id, CancellationToken ct = default)
        => throw new NotImplementedException("Not yet implemented");
}
