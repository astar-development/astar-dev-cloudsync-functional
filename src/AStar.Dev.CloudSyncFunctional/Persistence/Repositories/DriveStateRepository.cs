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
    public async Task<Option<DriveStateEntity>> GetByAccountAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var entity = await context.DriveStates.FindAsync([accountId], cancellationToken).ConfigureAwait(false);

        return entity is null
            ? new None<DriveStateEntity>()
            : new Some<DriveStateEntity>(entity);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertAsync(DriveStateEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.DriveStates.FindAsync([entity.AccountId], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.DriveStates.Add(entity);
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
}
