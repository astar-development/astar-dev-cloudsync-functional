using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class SyncRuleRepository(IDbContextFactory<AppDbContext> dbFactory) : ISyncRuleRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SyncRule>> GetByAccountAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = await context.SyncRules
            .AsNoTracking()
            .Where(r => r.AccountId == accountId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return entities.Select(ToSyncRule).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<AccountId, IReadOnlyList<SyncRule>>> GetAllByAccountIdsAsync(IEnumerable<AccountId> accountIds, CancellationToken cancellationToken = default)
    {
        var ids = accountIds.ToHashSet();
        if (ids.Count == 0)
            return new Dictionary<AccountId, IReadOnlyList<SyncRule>>();

        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = await context.SyncRules
            .AsNoTracking()
            .Where(r => ids.Contains(r.AccountId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return entities
            .GroupBy(r => r.AccountId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<SyncRule>)g.Select(ToSyncRule).ToList());
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(SyncRuleEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncRules.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.SyncRules.Add(entity);
            else
                context.Entry(existing).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Default;
        }
        catch (DbUpdateConcurrencyException)
        {
            return PersistenceErrorFactory.ConcurrencyConflict();
        }
        catch (DbUpdateException ex)
        {
            return PersistenceErrorFactory.Unexpected(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> DeleteAsync(SyncRuleId id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncRules.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                context.SyncRules.Remove(existing);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return Unit.Default;
        }
        catch (DbUpdateException ex)
        {
            return PersistenceErrorFactory.Unexpected(ex.Message);
        }
    }

    private static SyncRule ToSyncRule(SyncRuleEntity entity) =>
        entity.RuleType == RuleType.Include
            ? SyncRuleFactory.CreateInclude(entity.RemotePath)
            : SyncRuleFactory.CreateExclude(entity.RemotePath);
}
