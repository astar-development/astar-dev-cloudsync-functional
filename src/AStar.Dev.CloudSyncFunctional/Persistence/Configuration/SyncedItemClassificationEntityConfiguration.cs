using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="SyncedItemClassificationEntity"/>.</summary>
public sealed class SyncedItemClassificationEntityConfiguration : IEntityTypeConfiguration<SyncedItemClassificationEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SyncedItemClassificationEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SyncedItemId).HasConversion(SqliteTypeConverters.SyncedItemIdConverter);
    }
}
