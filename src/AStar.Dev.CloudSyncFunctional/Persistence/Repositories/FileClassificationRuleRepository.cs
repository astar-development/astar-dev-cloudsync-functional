using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class FileClassificationRuleRepository(IDbContextFactory<AppDbContext> dbFactory) : IFileClassificationRuleRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileClassificationRuleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.FileClassificationRules.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(FileClassificationRuleEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.FileClassificationRules.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.FileClassificationRules.Add(entity);
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
    public async Task<Result<Unit, PersistenceError>> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.FileClassificationRules.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                context.FileClassificationRules.Remove(existing);
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
