using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for a classification applied to a synced item.</summary>
public sealed class SyncedItemClassificationEntity
{
    /// <summary>Gets or sets the identifier of the classification record.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the synced item this classification belongs to.</summary>
    public SyncedItemId SyncedItemId { get; set; }

    /// <summary>Gets or sets the classification label.</summary>
    public string Classification { get; set; } = string.Empty;
}
