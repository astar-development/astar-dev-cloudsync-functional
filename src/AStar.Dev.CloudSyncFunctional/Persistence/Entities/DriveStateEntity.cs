using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for the cached drive state of an account.</summary>
public sealed class DriveStateEntity
{
    /// <summary>Gets or sets the account identifier (primary key and foreign key).</summary>
    public AccountId AccountId { get; set; }

    /// <summary>Gets or sets the delta link for incremental change enumeration.</summary>
    public string DeltaLink { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp of the last delta check.</summary>
    public DateTimeOffset LastCheckedAt { get; set; }
}
