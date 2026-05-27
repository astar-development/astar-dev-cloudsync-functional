using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Configuration;

/// <summary>EF Core entity configuration for <see cref="FileClassificationRuleEntity"/>.</summary>
public sealed class FileClassificationRuleEntityConfiguration : IEntityTypeConfiguration<FileClassificationRuleEntity>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileClassificationRuleEntity> builder)
        => builder.HasKey(e => e.Id);
}
