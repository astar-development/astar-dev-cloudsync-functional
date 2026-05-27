using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="DriveStateEntity"/>.</summary>
public sealed class DriveStateEntityConfiguration : IEntityTypeConfiguration<DriveStateEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<DriveStateEntity> builder)
    {
        builder.HasKey(e => e.AccountId);
        builder.Property(e => e.AccountId).HasConversion(SqliteTypeConverters.AccountIdConverter);
        builder.Property(e => e.LastCheckedAt).HasConversion(SqliteTypeConverters.DateTimeOffsetToTicks);
        builder.HasOne<AccountEntity>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
