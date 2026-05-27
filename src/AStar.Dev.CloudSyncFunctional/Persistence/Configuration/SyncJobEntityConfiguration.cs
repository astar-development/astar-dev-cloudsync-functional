using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="SyncJobEntity"/>.</summary>
public sealed class SyncJobEntityConfiguration : IEntityTypeConfiguration<SyncJobEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SyncJobEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasConversion(SqliteTypeConverters.SyncJobIdConverter);
        builder.Property(e => e.AccountId).HasConversion(SqliteTypeConverters.AccountIdConverter);
    }
}
