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
    public async Task<Option<SyncedItemEntity, PersistenceError>> GetByIdAsync(SyncedItemId id, CancellationToken ct = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var entity = await context.SyncedItems.FindAsync([id], ct).ConfigureAwait(false);

        return entity is null
            ? new None<SyncedItemEntity, PersistenceError>(PersistenceErrorFactory.Unexpected("Synced item not found."))
            : new Some<SyncedItemEntity, PersistenceError>(entity);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SyncedItemEntity>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        return await context.SyncedItems
            .AsNoTracking()
            .Where(i => i.AccountId == accountId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(SyncedItemEntity entity, CancellationToken ct = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var existing = await context.SyncedItems.FindAsync([entity.Id], ct).ConfigureAwait(false);
            if (existing is null)
                context.SyncedItems.Add(entity);
            else
                context.Entry(existing).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);

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
    public async Task<Result<Unit, PersistenceError>> DeleteAsync(SyncedItemId id, CancellationToken ct = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var existing = await context.SyncedItems.FindAsync([id], ct).ConfigureAwait(false);
            if (existing is not null)
            {
                context.SyncedItems.Remove(existing);
                await context.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            return new Ok<Unit, PersistenceError>(Unit.Default);
        }
        catch (DbUpdateException ex)
        {
            return new Fail<Unit, PersistenceError>(PersistenceErrorFactory.Unexpected(ex.Message));
        }
    }
}
