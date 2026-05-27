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
    public async Task<IReadOnlyList<SyncRuleEntity>> GetByAccountAsync(AccountId accountId, CancellationToken ct = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        return await context.SyncRules
            .AsNoTracking()
            .Where(r => r.AccountId == accountId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(SyncRuleEntity entity, CancellationToken ct = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var existing = await context.SyncRules.FindAsync([entity.Id], ct).ConfigureAwait(false);
            if (existing is null)
                context.SyncRules.Add(entity);
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
    public async Task<Result<Unit, PersistenceError>> DeleteAsync(SyncRuleId id, CancellationToken ct = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var existing = await context.SyncRules.FindAsync([id], ct).ConfigureAwait(false);
            if (existing is not null)
            {
                context.SyncRules.Remove(existing);
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
