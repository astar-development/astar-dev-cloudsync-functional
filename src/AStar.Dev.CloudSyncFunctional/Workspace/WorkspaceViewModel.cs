using System;
using System.Collections.ObjectModel;
using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.FolderTree;
using ReactiveUI;

namespace AStar.Dev.CloudSyncFunctional.Workspace;

/// <summary>Root view-model for the application workspace. Holds all accounts and summary statistics.</summary>
public class WorkspaceViewModel : ReactiveObject
{
    /// <summary>Gets all cloud storage accounts registered in the workspace.</summary>
    public ObservableCollection<AccountViewModel> Accounts { get; } = BuildAccounts();

    /// <summary>Gets or sets the currently selected account.</summary>
    public AccountViewModel? SelectedAccount { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets hourly transfer buckets for today (24 values, index = hour).</summary>
    public int[] TodayBuckets { get; } =
    [
        112, 87, 65, 73, 91, 140, 155, 178,
        160, 132, 120, 115, 108, 95, 130, 144,
        158, 175, 163, 149, 450, 620, 710, 780
    ];

    /// <summary>Gets the total data transferred today in gigabytes.</summary>
    public double TodayGb { get; } = 1.24;

    /// <summary>Gets the total number of files synchronised today.</summary>
    public int TodayFileCount { get; } = 384;

    /// <summary>Gets the current transfer rate as a display string.</summary>
    public string CurrentRate { get; } = "142 KB/s";

    /// <summary>Initialises a new <see cref="WorkspaceViewModel"/> and selects the first account.</summary>
    public WorkspaceViewModel()
    {
        SelectedAccount = Accounts[0];
    }

    private static ObservableCollection<AccountViewModel> BuildAccounts() =>
    [
        BuildPersonalOneDrive(),
        BuildWorkGoogleDrive(),
        BuildJaneGoogleDrive(),
        BuildDropboxSideProject()
    ];

    private static AccountViewModel BuildPersonalOneDrive() =>
        new()
        {
            Kind = ProviderKind.OneDrive,
            Name = "Personal",
            Email = "jason@outlook.com",
            UsedBytes = 120L * 1024 * 1024 * 1024,
            TotalBytes = 1024L * 1024 * 1024 * 1024,
            FolderCount = 12,
            Status = SyncStatus.Ok,
            Folders =
            [
                new FolderNode { Path = "/Pictures", Name = "Pictures", Depth = 0, ChildCount = 4, SizeBytes = 35_000_000_000L, LastSync = DateTimeOffset.Now, SelectionState = CheckState.On, IsExpanded = false, IsSyncing = false },
                new FolderNode { Path = "/Documents", Name = "Documents", Depth = 0, ChildCount = 6, SizeBytes = 12_400_000_000L, LastSync = DateTimeOffset.Now.AddMinutes(-15), SelectionState = CheckState.On, IsExpanded = false, IsSyncing = false },
                new FolderNode { Path = "/Music", Name = "Music", Depth = 0, ChildCount = 2, SizeBytes = 8_200_000_000L, LastSync = DateTimeOffset.Now.AddHours(-2), SelectionState = CheckState.Off, IsExpanded = false, IsSyncing = false }
            ]
        };

    private static AccountViewModel BuildWorkGoogleDrive() =>
        new()
        {
            Kind = ProviderKind.GoogleDrive,
            Name = "Work",
            Email = "jason.barden@work.com",
            UsedBytes = 45L * 1024 * 1024 * 1024,
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FolderCount = 8,
            Status = SyncStatus.Syncing,
            Folders = BuildWorkFolderTree()
        };

    private static AccountViewModel BuildJaneGoogleDrive() =>
        new()
        {
            Kind = ProviderKind.GoogleDrive,
            Name = "jane.dev",
            Email = "jane@dev.io",
            UsedBytes = 33L * 1024 * 1024 * 1024,
            TotalBytes = 50L * 1024 * 1024 * 1024,
            FolderCount = 5,
            Status = SyncStatus.Ok,
            Folders =
            [
                new FolderNode { Path = "/Projects", Name = "Projects", Depth = 0, ChildCount = 3, SizeBytes = 18_000_000_000L, LastSync = DateTimeOffset.Now.AddMinutes(-5), SelectionState = CheckState.On, IsExpanded = false, IsSyncing = false },
                new FolderNode { Path = "/Shared", Name = "Shared", Depth = 0, ChildCount = 2, SizeBytes = 4_800_000_000L, LastSync = DateTimeOffset.Now.AddHours(-1), SelectionState = CheckState.Mixed, IsExpanded = false, IsSyncing = false }
            ]
        };

    private static AccountViewModel BuildDropboxSideProject() =>
        new()
        {
            Kind = ProviderKind.Dropbox,
            Name = "Side Project",
            Email = "jason@sideproject.io",
            UsedBytes = 8L * 1024 * 1024 * 1024,
            TotalBytes = 20L * 1024 * 1024 * 1024,
            FolderCount = 3,
            Status = SyncStatus.Warn,
            Folders =
            [
                new FolderNode { Path = "/Releases", Name = "Releases", Depth = 0, ChildCount = 2, SizeBytes = 5_300_000_000L, LastSync = DateTimeOffset.Now.AddHours(-3), SelectionState = CheckState.On, IsExpanded = false, IsSyncing = false },
                new FolderNode { Path = "/Assets", Name = "Assets", Depth = 0, ChildCount = 1, SizeBytes = 1_900_000_000L, LastSync = DateTimeOffset.Now.AddDays(-1), SelectionState = CheckState.Off, IsExpanded = false, IsSyncing = false }
            ]
        };

    private static ObservableCollection<FolderNode> BuildWorkFolderTree()
    {
        var documentsNode = new FolderNode
        {
            Path = "/Documents",
            Name = "Documents",
            Depth = 0,
            ChildCount = 2,
            SizeBytes = 4_515_000_000L,
            LastSync = DateTimeOffset.Now,
            SelectionState = CheckState.On,
            IsExpanded = true,
            IsSyncing = false,
            Children =
            [
                new FolderNode { Path = "/Documents/Contracts2025", Name = "Contracts 2025", Depth = 1, ChildCount = 2, SizeBytes = 228_000_000L, LastSync = DateTimeOffset.Now.AddMinutes(-2), SelectionState = CheckState.On, IsExpanded = false, IsSyncing = false }
            ]
        };

        var engineeringNode = new FolderNode
        {
            Path = "/Engineering",
            Name = "Engineering",
            Depth = 0,
            ChildCount = 3,
            SizeBytes = 20_275_200_000L,
            LastSync = DateTimeOffset.Now.AddSeconds(-40),
            SelectionState = CheckState.Mixed,
            IsExpanded = true,
            IsSyncing = true,
            Children =
            [
                new FolderNode { Path = "/Engineering/src", Name = "src/", Depth = 1, ChildCount = 142, SizeBytes = 2_255_000_000L, LastSync = DateTimeOffset.Now, SelectionState = CheckState.On, IsExpanded = false, IsSyncing = true },
                new FolderNode { Path = "/Engineering/archive", Name = "archive/", Depth = 1, ChildCount = 12, SizeBytes = 15_247_000_000L, LastSync = DateTimeOffset.MinValue, SelectionState = CheckState.Off, IsExpanded = false, IsSyncing = false },
                new FolderNode { Path = "/Engineering/sandbox", Name = "sandbox/", Depth = 1, ChildCount = 8, SizeBytes = 443_000_000L, LastSync = DateTimeOffset.Now.AddHours(-4), SelectionState = CheckState.On, IsExpanded = false, IsSyncing = false }
            ]
        };

        return [documentsNode, engineeringNode];
    }
}
