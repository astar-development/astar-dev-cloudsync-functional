using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="AccountEntity"/>.</summary>
public sealed class AccountEntityConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasConversion(SqliteTypeConverters.AccountIdConverter);
        builder.Property(e => e.DriveId).HasConversion(SqliteTypeConverters.DriveIdConverter);
        builder.ComplexProperty(e => e.Profile, p =>
        {
            p.Property(prof => prof.DisplayName).HasConversion(SqliteTypeConverters.DisplayNameConverter).HasColumnName("DisplayName");
            p.Property(prof => prof.Email).HasConversion(SqliteTypeConverters.EmailAddressConverter).HasColumnName("Email");
        });
        builder.ComplexProperty(e => e.SyncConfig, sc =>
        {
            sc.Property(c => c.LocalSyncPath).HasConversion(SqliteTypeConverters.LocalSyncPathConverter).HasColumnName("LocalSyncPath");
            sc.Property(c => c.WorkerCount).HasColumnName("WorkerCount");
        });
    }
}
