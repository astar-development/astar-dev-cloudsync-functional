using AStar.Dev.CloudSyncFunctional.Auth;

namespace AStar.Dev.CloudSyncFunctional.Domain;

/// <summary>Represents an authenticated OneDrive account and its sync configuration.</summary>
public sealed class OneDriveAccount
{
    /// <summary>Gets the MSAL HomeAccountId identifier.</summary>
    public AccountId AccountId { get; init; } = AccountId.Create(string.Empty);

    /// <summary>Gets the account's display name and email.</summary>
    public AccountProfile Profile { get; init; } = new(string.Empty, string.Empty);

    /// <summary>Gets or sets whether this account is active for sync.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets the Graph drive ID for this account's OneDrive.</summary>
    public string? DriveId { get; init; }

    /// <summary>Gets the folders selected for sync, carrying both Graph item ID and display name.</summary>
    public IReadOnlyList<SelectedFolder> SelectedFolders { get; init; } = [];
}
