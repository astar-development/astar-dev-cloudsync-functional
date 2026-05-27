using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="SyncConflictEntity"/>.</summary>
public sealed class SyncConflictEntityConfiguration : IEntityTypeConfiguration<SyncConflictEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SyncConflictEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasConversion(SqliteTypeConverters.SyncConflictIdConverter);
        builder.Property(e => e.AccountId).HasConversion(SqliteTypeConverters.AccountIdConverter);
        builder.Property(e => e.LocalModifiedAt).HasConversion(SqliteTypeConverters.DateTimeOffsetToTicks);
        builder.Property(e => e.RemoteModifiedAt).HasConversion(SqliteTypeConverters.DateTimeOffsetToTicks);
        builder.HasOne<AccountEntity>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
