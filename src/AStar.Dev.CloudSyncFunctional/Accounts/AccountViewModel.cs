using System.Collections.ObjectModel;
using AStar.Dev.CloudSyncFunctional.FolderTree;
using ReactiveUI;

namespace AStar.Dev.CloudSyncFunctional.Accounts;

/// <summary>Represents a single cloud storage account and its folder tree.</summary>
public class AccountViewModel : ReactiveObject
{
    /// <summary>Gets the provider type for this account.</summary>
    public ProviderKind Kind { get; init; }

    /// <summary>Gets the MSAL HomeAccountId identifier for this account.</summary>
    public string AccountId { get; init; } = string.Empty;

    /// <summary>Gets the display name for this account.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the email address associated with this account.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the number of bytes currently used on this account.</summary>
    public long UsedBytes { get; init; }

    /// <summary>Gets the total storage capacity of this account in bytes.</summary>
    public long TotalBytes { get; init; }

    /// <summary>Gets the total number of folders tracked for this account.</summary>
    public int FolderCount { get; init; }

    /// <summary>Gets or sets the current synchronisation status.</summary>
    public SyncStatus Status
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets whether this account is currently selected in the sidebar.</summary>
    public bool IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets the root-level folder nodes for this account.</summary>
    public ObservableCollection<FolderNode> Folders { get; init; } = [];
}
