using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.FolderTree;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Recovery;
using AStar.Dev.CloudSyncFunctional.Settings;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.CloudSyncFunctional.Wizard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;
using PersistenceAccountId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.AccountId;

namespace AStar.Dev.CloudSyncFunctional.Workspace;

/// <summary>Root view-model for the application workspace. Holds all accounts and summary statistics.</summary>
public class WorkspaceViewModel : ReactiveObject, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAccountRepository? _accountRepository;
    private readonly ISyncRecoveryService? _recoveryService;
    private readonly ISyncRuleRepository? _syncRuleRepository;
    private readonly ISyncScheduler? _syncScheduler;
    private readonly ILogger<WorkspaceViewModel>? _logger;
    private readonly CompositeDisposable _disposables = new();

    /// <summary>Gets all cloud storage accounts registered in the workspace.</summary>
    public ObservableCollection<AccountViewModel> Accounts { get; }

    /// <summary>Gets or sets the currently selected account.</summary>
    public AccountViewModel? SelectedAccount
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            foreach (var account in Accounts)
                account.IsSelected = account == value;
        }
    }

    /// <summary>Gets or sets the current overlay content (e.g. the add-account wizard). Null means no overlay.</summary>
    public ReactiveObject? CurrentOverlay
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets a value indicating whether any syncs were interrupted and need recovery.</summary>
    public bool HasInterruptedSyncs
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets a value indicating whether the last sync command produced an error.</summary>
    public bool HasError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets the error message from the last failed sync command.</summary>
    public string ErrorMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>Gets the command that opens the add-account wizard overlay.</summary>
    public ReactiveCommand<RxUnit, RxUnit> OpenAddAccountWizard { get; }

    /// <summary>Gets the command that opens the settings overlay.</summary>
    public ReactiveCommand<RxUnit, RxUnit> OpenSettings { get; }

    /// <summary>Gets the command that triggers an immediate sync for the currently selected account.</summary>
    public ReactiveCommand<RxUnit, RxUnit> TriggerSync { get; private set; } = null!;

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

    /// <summary>Gets the download rate display string for the status bar.</summary>
    public string DownloadRate { get; } = "↓ 142 KB/s";

    /// <summary>Gets the upload rate display string for the status bar.</summary>
    public string UploadRate { get; } = "↑ 0 B/s";

    /// <summary>Gets the queue summary display string for the status bar.</summary>
    public string QueueSummary { get; } = "3 files in queue · 188 MB";

    /// <summary>Gets the application version display string for the status bar.</summary>
    public string Version { get; } = "v2.4.1 · linux-x64";

    /// <summary>Gets a formatted subtitle summarising account count and total storage capacity.</summary>
    public string WorkspaceSubtitle => $"{Accounts.Count} accounts · {Accounts.Sum(a => a.TotalBytes) / 1_099_511_627_776.0:F1} TB total";

    /// <summary>Initialises a new <see cref="WorkspaceViewModel"/> using the provided service provider and account repository (runtime path).</summary>
    /// <param name="serviceProvider">The DI container used to resolve the wizard ViewModel on demand.</param>
    /// <param name="accountRepository">Repository used to load persisted accounts on startup.</param>
    /// <param name="recoveryService">Optional recovery service used to detect interrupted syncs on startup.</param>
    /// <param name="syncRuleRepository">Optional repository used to load sync rules (folders) per account.</param>
    /// <param name="syncScheduler">Optional scheduler used to trigger on-demand sync.</param>
    /// <param name="logger">Optional logger for error reporting.</param>
    public WorkspaceViewModel(IServiceProvider serviceProvider, IAccountRepository accountRepository, ISyncRecoveryService? recoveryService = null, ISyncRuleRepository? syncRuleRepository = null, ISyncScheduler? syncScheduler = null, ILogger<WorkspaceViewModel>? logger = null)
    {
        _serviceProvider = serviceProvider;
        _accountRepository = accountRepository;
        _recoveryService = recoveryService;
        _syncRuleRepository = syncRuleRepository;
        _syncScheduler = syncScheduler;
        _logger = logger;
        Accounts = [];
        OpenAddAccountWizard = ReactiveCommand.Create(ExecuteOpenAddAccountWizard);
        OpenSettings = ReactiveCommand.Create(ExecuteOpenSettings);
        InitializeCommands();
    }

    /// <summary>Initialises a new <see cref="WorkspaceViewModel"/> with design-time data.</summary>
    /// <param name="serviceProvider">The DI container used to resolve the wizard ViewModel on demand.</param>
    public WorkspaceViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Accounts = BuildAccounts();
        SelectedAccount = Accounts[0];
        OpenAddAccountWizard = ReactiveCommand.Create(ExecuteOpenAddAccountWizard);
        OpenSettings = ReactiveCommand.Create(ExecuteOpenSettings);
        InitializeCommands();
    }

    /// <summary>Initialises a new <see cref="WorkspaceViewModel"/> with no DI services (design-time use).</summary>
    public WorkspaceViewModel() : this(EmptyServiceProvider.Instance)
    {
    }

    /// <summary>Loads persisted accounts from the database and populates <see cref="Accounts"/>.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when accounts are loaded and added to the collection.</returns>
    public async Task LoadPersistedAccountsAsync(CancellationToken cancellationToken = default)
    {
        if (_accountRepository is null)
            return;

        var entities = await _accountRepository.GetAllAsync(cancellationToken);
        var accountIds = entities.Select(e => e.Id);
        IReadOnlyDictionary<PersistenceAccountId, IReadOnlyList<SyncRule>> rulesByAccount = _syncRuleRepository is not null
            ? await _syncRuleRepository.GetAllByAccountIdsAsync(accountIds, cancellationToken)
            : new Dictionary<PersistenceAccountId, IReadOnlyList<SyncRule>>();

        foreach (var entity in entities)
        {
            var syncRules = rulesByAccount.TryGetValue(entity.Id, out var rules) ? rules : [];
            Accounts.Add(MapToViewModel(entity, syncRules));
        }

        if (Accounts.Count > 0 && SelectedAccount is null)
            SelectedAccount = Accounts[0];

        if (_recoveryService is not null)
        {
            var interrupted = await _recoveryService.DetectAsync(cancellationToken);
            HasInterruptedSyncs = interrupted.Count > 0;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _disposables.Dispose();

    private void InitializeCommands()
    {
        var canSync = this.WhenAnyValue(
            x => x.SelectedAccount,
            a => a is not null && !string.IsNullOrEmpty(a.AccountId) && _syncScheduler is not null);

        TriggerSync = ReactiveCommand.CreateFromTask(ExecuteTriggerSyncAsync, canSync);

        _disposables.Add(TriggerSync.ThrownExceptions
            .Subscribe(ex =>
            {
                HasError = true;
                ErrorMessage = ex.Message;
                _logger?.LogError(ex, "TriggerSync failed for account {AccountId}", SelectedAccount?.AccountId);
            }));
    }

    private void ExecuteOpenAddAccountWizard()
    {
        var wizard = _serviceProvider.GetRequiredService<AddAccountWizardViewModel>();
        wizard.Completed += OnWizardCompleted;
        wizard.Cancelled += OnWizardCancelled;
        CurrentOverlay = wizard;
    }

    private void ExecuteOpenSettings()
    {
        var settings = _serviceProvider.GetRequiredService<SettingsViewModel>();
        settings.Closed += OnSettingsClosed;
        CurrentOverlay = settings;
    }

    private void OnSettingsClosed(object? sender, EventArgs e)
    {
        if (sender is SettingsViewModel settings)
            settings.Closed -= OnSettingsClosed;
        CurrentOverlay = null;
    }

    private void OnWizardCompleted(object? sender, OneDriveAccount account)
    {
        DetachAndDisposeWizard(sender);
        CurrentOverlay = null;
        Accounts.Add(new AccountViewModel
        {
            Kind = ProviderKind.OneDrive,
            AccountId = account.AccountId.Value,
            Name = account.Profile.DisplayName,
            Email = account.Profile.Email,
            Status = SyncStatus.Ok,
            FolderCount = account.SelectedFolders.Count,
            Folders = [.. account.SelectedFolders.Select(f => new FolderNode
            {
                Path = $"/{f.Name}",
                Name = f.Name,
                Depth = 0,
                SelectionState = CheckState.On,
                LastSync = DateTimeOffset.MinValue
            })]
        });
        SelectedAccount = Accounts[^1];
        this.RaisePropertyChanged(nameof(WorkspaceSubtitle));
    }

    private static AccountViewModel MapToViewModel(AccountEntity entity, IReadOnlyList<SyncRule> syncRules)
    {
        var includedFolders = syncRules
            .Where(r => r.RuleType == RuleType.Include)
            .Select(r => new FolderNode
            {
                Path = r.RemotePath,
                Name = r.RemotePath.TrimStart('/'),
                Depth = 0,
                SelectionState = CheckState.On,
                LastSync = DateTimeOffset.MinValue
            })
            .ToList();

        return new AccountViewModel
        {
            Kind = ProviderKind.OneDrive,
            AccountId = entity.Id.Value,
            Name = entity.Profile.DisplayName.Value,
            Email = entity.Profile.Email.Value,
            Status = SyncStatus.Ok,
            FolderCount = includedFolders.Count,
            Folders = [.. includedFolders]
        };
    }

    private Task ExecuteTriggerSyncAsync(CancellationToken cancellationToken)
        => _syncScheduler!.TriggerAccountAsync(SelectedAccount!.AccountId, cancellationToken);

    private void OnWizardCancelled(object? sender, EventArgs e)
    {
        DetachAndDisposeWizard(sender);
        CurrentOverlay = null;
    }

    private static void DetachAndDisposeWizard(object? sender)
    {
        if (sender is AddAccountWizardViewModel wizard)
            wizard.Dispose();
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

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static EmptyServiceProvider Instance { get; } = new();

        /// <summary>Returns null for all service types.</summary>
        /// <param name="serviceType">The requested service type.</param>
        /// <returns>Always null.</returns>
        public object? GetService(Type serviceType) => null;
    }
}
