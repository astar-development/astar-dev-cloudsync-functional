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
    public async Task<Option<AccountEntity>> GetByIdAsync(AccountId id, CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var entity = await context.Accounts.FindAsync([id], cancellationToken).ConfigureAwait(false);

        return entity is null
            ? new None<AccountEntity>()
            : new Some<AccountEntity>(entity);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AccountEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.Accounts.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(AccountEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.Accounts.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.Accounts.Add(entity);
            else
                context.Entry(existing).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new Ok<Unit, PersistenceError>(Unit.Default);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new Fail<Unit, PersistenceError>(PersistenceErrorFactory.ConcurrencyConflict());
        }
        catch (DbUpdateException ex)
        {
            return new Fail<Unit, PersistenceError>(PersistenceErrorFactory.Unexpected(ex.Message));
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> DeleteAsync(AccountId id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.Accounts.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                context.Accounts.Remove(existing);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Ok<Unit, PersistenceError>(Unit.Default);
        }
        catch (DbUpdateException ex)
        {
            return new Fail<Unit, PersistenceError>(PersistenceErrorFactory.Unexpected(ex.Message));
        }
    }
}
