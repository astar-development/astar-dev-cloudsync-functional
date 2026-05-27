using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for a detected sync conflict.</summary>
public sealed class SyncConflictEntity
{
    /// <summary>Gets or sets the sync conflict identifier.</summary>
    public SyncConflictId Id { get; set; }

    /// <summary>Gets or sets the account this conflict belongs to.</summary>
    public AccountId AccountId { get; set; }

    /// <summary>Gets or sets the remote OneDrive item identifier involved in the conflict.</summary>
    public string RemoteItemId { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp of the local file at the time of conflict detection.</summary>
    public DateTimeOffset LocalModifiedAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the remote file at the time of conflict detection.</summary>
    public DateTimeOffset RemoteModifiedAt { get; set; }

    /// <summary>Gets or sets the resolution state — "Pending" or "Resolved".</summary>
    public string State { get; set; } = "Pending";
}
