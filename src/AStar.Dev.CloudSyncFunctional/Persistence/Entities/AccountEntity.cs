using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for an OneDrive account.</summary>
public sealed class AccountEntity
{
    /// <summary>Gets or sets the MSAL HomeAccountId identifier.</summary>
    public AccountId Id { get; set; }

    /// <summary>Gets or sets the account profile (display name, email).</summary>
    public AccountProfileEntity Profile { get; set; } = new();

    /// <summary>Gets or sets whether this account is active for sync.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the Graph drive ID.</summary>
    public DriveId DriveId { get; set; }

    /// <summary>Gets or sets the sync configuration.</summary>
    public AccountSyncConfig SyncConfig { get; set; } = new();

    /// <summary>Gets or sets the UTC timestamp of the last successful sync.</summary>
    public DateTimeOffset? LastSyncedAt { get; set; }
}
