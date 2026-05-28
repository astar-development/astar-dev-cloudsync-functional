using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Repositories;

/// <inheritdoc/>
public sealed class SyncRepository(IDbContextFactory<AppDbContext> dbFactory) : ISyncRepository
{
    private const string PendingState = "Pending";
    private const string ResolvedState = "Resolved";
    private const string CompletedStatus = "Completed";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SyncConflictEntity>> GetPendingConflictsAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.SyncConflicts
            .AsNoTracking()
            .Where(c => c.AccountId == accountId && c.State == PendingState)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, PersistenceError>> UpsertConflictAsync(SyncConflictEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncConflicts.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.SyncConflicts.Add(entity);
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
    public async Task<Result<Unit, PersistenceError>> ResolveConflictAsync(SyncConflictId id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncConflicts.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                existing.State = ResolvedState;
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

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
    public async Task<Result<Unit, PersistenceError>> UpsertJobAsync(SyncJobEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var existing = await context.SyncJobs.FindAsync([entity.Id], cancellationToken).ConfigureAwait(false);
            if (existing is null)
                context.SyncJobs.Add(entity);
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
    public async Task<Result<Unit, PersistenceError>> ClearCompletedJobsAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            var completed = await context.SyncJobs
                .Where(j => j.AccountId == accountId && j.Status == CompletedStatus)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            context.SyncJobs.RemoveRange(completed);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new Ok<Unit, PersistenceError>(Unit.Default);
        }
        catch (DbUpdateException ex)
        {
            return new Fail<Unit, PersistenceError>(PersistenceErrorFactory.Unexpected(ex.Message));
        }
    }
}
