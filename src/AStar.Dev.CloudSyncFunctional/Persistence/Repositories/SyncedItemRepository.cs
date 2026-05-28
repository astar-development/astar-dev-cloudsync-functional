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
    public async Task<Option<SyncedItemEntity>> GetByIdAsync(SyncedItemId id, CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var entity = await context.SyncedItems.FindAsync([id], cancellationToken).ConfigureAwait(false);

        return entity is null
            ? new None<SyncedItemEntity>()
            : new Some<SyncedItemEntity>(entity);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SyncedItemEntity>> GetByAccountAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.SyncedItems
            .AsNoTracking()
            .Where(i => i.AccountId == accountId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(SyncedItemEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncedItems.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.SyncedItems.Add(entity);
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
    public async Task<Result<Unit, PersistenceError>> DeleteAsync(SyncedItemId id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncedItems.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                context.SyncedItems.Remove(existing);
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
