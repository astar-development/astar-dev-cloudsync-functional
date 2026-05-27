using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for a synced file or folder item.</summary>
public sealed class SyncedItemEntity
{
    /// <summary>Gets or sets the synced item identifier.</summary>
    public SyncedItemId Id { get; set; }

    /// <summary>Gets or sets the account this item belongs to.</summary>
    public AccountId AccountId { get; set; }

    /// <summary>Gets or sets the remote OneDrive path of this item.</summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the local file system path of this item.</summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp of the last known remote modification.</summary>
    public DateTimeOffset RemoteModifiedAt { get; set; }

    /// <summary>Gets or sets the remote eTag for conflict detection.</summary>
    public string? ETag { get; set; }

    /// <summary>Gets or sets whether this item is a folder.</summary>
    public bool IsFolder { get; set; }
}
