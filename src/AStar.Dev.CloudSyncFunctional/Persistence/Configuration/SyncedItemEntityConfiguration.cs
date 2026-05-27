using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="SyncedItemEntity"/>.</summary>
public sealed class SyncedItemEntityConfiguration : IEntityTypeConfiguration<SyncedItemEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SyncedItemEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasConversion(SqliteTypeConverters.SyncedItemIdConverter);
        builder.Property(e => e.AccountId).HasConversion(SqliteTypeConverters.AccountIdConverter);
    }
}
