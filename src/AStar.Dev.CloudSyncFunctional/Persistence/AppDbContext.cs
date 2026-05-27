using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Persistence;

/// <summary>EF Core database context for the cloud sync application.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Gets the accounts dataset.</summary>
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    /// <summary>Gets the sync rules dataset.</summary>
    public DbSet<SyncRuleEntity> SyncRules => Set<SyncRuleEntity>();

    /// <summary>Gets the synced items dataset.</summary>
    public DbSet<SyncedItemEntity> SyncedItems => Set<SyncedItemEntity>();

    /// <summary>Gets the synced item classifications dataset.</summary>
    public DbSet<SyncedItemClassificationEntity> SyncedItemClassifications => Set<SyncedItemClassificationEntity>();

    /// <summary>Gets the file classification rules dataset.</summary>
    public DbSet<FileClassificationRuleEntity> FileClassificationRules => Set<FileClassificationRuleEntity>();

    /// <summary>Gets the sync jobs dataset.</summary>
    public DbSet<SyncJobEntity> SyncJobs => Set<SyncJobEntity>();

    /// <summary>Gets the sync conflicts dataset.</summary>
    public DbSet<SyncConflictEntity> SyncConflicts => Set<SyncConflictEntity>();

    /// <summary>Gets the drive states dataset.</summary>
    public DbSet<DriveStateEntity> DriveStates => Set<DriveStateEntity>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
