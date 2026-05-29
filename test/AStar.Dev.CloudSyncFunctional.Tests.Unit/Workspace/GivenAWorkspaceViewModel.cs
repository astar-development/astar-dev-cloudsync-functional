using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Recovery;
using AStar.Dev.CloudSyncFunctional.Settings;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;
using AStar.Dev.CloudSyncFunctional.Wizard;
using AStar.Dev.CloudSyncFunctional.Workspace;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using DriveId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.DriveId;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Workspace;

public class GivenAWorkspaceViewModel : IClassFixture<ReactiveUiFixture>
{
    [Fact]
    public void when_constructed_then_accounts_contains_four_entries()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts.Count.ShouldBe(4);
    }

    [Fact]
    public void when_constructed_then_accounts_are_in_provider_order()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts[0].Kind.ShouldBe(ProviderKind.OneDrive);
        sut.Accounts[1].Kind.ShouldBe(ProviderKind.GoogleDrive);
        sut.Accounts[2].Kind.ShouldBe(ProviderKind.GoogleDrive);
        sut.Accounts[3].Kind.ShouldBe(ProviderKind.Dropbox);
    }

    [Fact]
    public void when_constructed_then_selected_account_is_first_account()
    {
        var sut = new WorkspaceViewModel();

        sut.SelectedAccount.ShouldBeSameAs(sut.Accounts[0]);
    }

    [Fact]
    public void when_constructed_then_first_account_is_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts[0].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void when_constructed_then_non_first_accounts_are_not_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts[1].IsSelected.ShouldBeFalse();
        sut.Accounts[2].IsSelected.ShouldBeFalse();
        sut.Accounts[3].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_today_buckets_has_twenty_four_entries()
    {
        var sut = new WorkspaceViewModel();

        sut.TodayBuckets.Length.ShouldBe(24);
    }

    [Fact]
    public void when_selected_account_is_set_then_property_changed_fires()
    {
        var sut = new WorkspaceViewModel();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.SelectedAccount = sut.Accounts[1];

        raisedProperties.ShouldContain(nameof(WorkspaceViewModel.SelectedAccount));
    }

    [Fact]
    public void when_selected_account_is_changed_then_new_account_is_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.SelectedAccount = sut.Accounts[2];

        sut.Accounts[2].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void when_selected_account_is_changed_then_previous_account_is_not_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.SelectedAccount = sut.Accounts[2];

        sut.Accounts[0].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_download_rate_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.DownloadRate.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_upload_rate_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.UploadRate.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_queue_summary_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.QueueSummary.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_version_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.Version.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_workspace_subtitle_contains_account_count()
    {
        var sut = new WorkspaceViewModel();

        sut.WorkspaceSubtitle.ShouldContain("4");
    }

    [Fact]
    public void when_constructed_then_workspace_subtitle_contains_total_storage()
    {
        var sut = new WorkspaceViewModel();

        sut.WorkspaceSubtitle.ShouldContain("TB");
    }

    [Fact]
    public void when_constructed_then_current_overlay_is_null()
    {
        var sut = new WorkspaceViewModel();

        sut.CurrentOverlay.ShouldBeNull();
    }

    [Fact]
    public void when_current_overlay_is_set_then_property_changed_fires()
    {
        var sut = new WorkspaceViewModel();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);
        var vm = Substitute.For<ReactiveObject>();

        sut.CurrentOverlay = vm;

        raisedProperties.ShouldContain(nameof(WorkspaceViewModel.CurrentOverlay));
    }

    [Fact]
    public void when_open_add_account_wizard_is_executed_then_current_overlay_is_set()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        var provider = services.BuildServiceProvider();

        var sut = new WorkspaceViewModel(provider);

        sut.OpenAddAccountWizard.Execute().Subscribe();

        sut.CurrentOverlay.ShouldNotBeNull();
        sut.CurrentOverlay.ShouldBeOfType<AddAccountWizardViewModel>();
    }

    [Fact]
    public void when_wizard_cancelled_event_fires_then_current_overlay_is_null()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenAddAccountWizard.Execute().Subscribe();

        var wizard = (AddAccountWizardViewModel)sut.CurrentOverlay!;
        wizard.Cancel.Execute().Subscribe();

        sut.CurrentOverlay.ShouldBeNull();
    }

    [Fact]
    public void when_wizard_completed_then_current_overlay_is_null()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var account = new OneDriveAccount { AccountId = CloudSyncFunctional.Auth.AccountId.Create("id"), Profile = new AccountProfile("Name", "email@x.com"), SelectedFolders = [] };
        onboarding.CompleteOnboardingAsync(Arg.Any<OneDriveAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveAccount, PersistenceError>>(new Ok<OneDriveAccount, PersistenceError>(account)));

        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenAddAccountWizard.Execute().Subscribe();

        var wizard = (AddAccountWizardViewModel)sut.CurrentOverlay!;
        wizard.SimulateCompleted(account);

        sut.CurrentOverlay.ShouldBeNull();
    }

    [Fact]
    public void when_wizard_completed_then_account_is_added_to_accounts()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var account = new OneDriveAccount { AccountId = CloudSyncFunctional.Auth.AccountId.Create("id"), Profile = new AccountProfile("New User", "new@x.com"), SelectedFolders = [new SelectedFolder("f1-id", "f1")] };
        onboarding.CompleteOnboardingAsync(Arg.Any<OneDriveAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveAccount, PersistenceError>>(new Ok<OneDriveAccount, PersistenceError>(account)));

        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenAddAccountWizard.Execute().Subscribe();

        var wizard = (AddAccountWizardViewModel)sut.CurrentOverlay!;
        wizard.SimulateCompleted(account);

        sut.Accounts.Count.ShouldBe(5);
    }

    [Fact]
    public async Task when_load_persisted_accounts_is_called_then_stored_account_appears_in_accounts()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>(
            [
                new AccountEntity
                {
                    Id = new Persistence.ValueObjects.AccountId("acc-1"),
                    Profile = new AccountProfileEntity { DisplayName = new DisplayName("Alice"), Email = new EmailAddress("alice@x.com") },
                    IsActive = true,
                    DriveId = new DriveId("drive-1"),
                    SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/alice/OneDrive"), WorkerCount = 8 }
                }
            ]));
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo);

        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        sut.Accounts.ShouldContain(a => a.Email == "alice@x.com");
    }

    [Fact]
    public async Task when_load_persisted_accounts_is_called_with_no_accounts_then_accounts_collection_is_empty()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([]));
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo);

        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        sut.Accounts.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_load_persisted_accounts_is_called_then_first_account_is_auto_selected()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>(
            [
                new AccountEntity
                {
                    Id = new Persistence.ValueObjects.AccountId("acc-1"),
                    Profile = new AccountProfileEntity { DisplayName = new DisplayName("Bob"), Email = new EmailAddress("bob@x.com") },
                    IsActive = true,
                    DriveId = new DriveId("drive-1"),
                    SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/bob/OneDrive"), WorkerCount = 8 }
                }
            ]));
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo);

        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        sut.SelectedAccount.ShouldNotBeNull();
        sut.SelectedAccount!.Email.ShouldBe("bob@x.com");
    }

    [Fact]
    public async Task when_interrupted_syncs_are_detected_then_has_interrupted_syncs_is_true()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([]));
        var recoveryService = Substitute.For<ISyncRecoveryService>();
        recoveryService.DetectAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<InterruptedSyncInfo>>(
            [
                new InterruptedSyncInfo(
                    new Persistence.ValueObjects.AccountId("acc-1"),
                    "Test User",
                    CanResume: true,
                    "Sync resumed from last checkpoint.")
            ]));
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo, recoveryService);

        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        sut.HasInterruptedSyncs.ShouldBeTrue();
    }

    [Fact]
    public async Task when_no_interrupted_syncs_then_has_interrupted_syncs_is_false()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([]));
        var recoveryService = Substitute.For<ISyncRecoveryService>();
        recoveryService.DetectAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<InterruptedSyncInfo>>([]));
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo, recoveryService);

        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        sut.HasInterruptedSyncs.ShouldBeFalse();
    }

    [Fact]
    public void when_wizard_completed_with_selected_folders_then_account_has_folder_node_per_selected_folder()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var account = new OneDriveAccount
        {
            AccountId = CloudSyncFunctional.Auth.AccountId.Create("acc-wiz"),
            Profile = new AccountProfile("Name", "email@x.com"),
            SelectedFolders = [new SelectedFolder("desktop-id", "Desktop")]
        };
        onboarding.CompleteOnboardingAsync(Arg.Any<OneDriveAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveAccount, PersistenceError>>(new Ok<OneDriveAccount, PersistenceError>(account)));
        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenAddAccountWizard.Execute().Subscribe();
        var wizard = (AddAccountWizardViewModel)sut.CurrentOverlay!;

        wizard.SimulateCompleted(account);

        var added = sut.Accounts[^1];
        added.Folders.Count.ShouldBe(1);
        added.Folders[0].Name.ShouldBe("Desktop");
    }

    [Fact]
    public void when_wizard_completed_then_added_account_has_account_id()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var account = new OneDriveAccount
        {
            AccountId = CloudSyncFunctional.Auth.AccountId.Create("acc-wiz"),
            Profile = new AccountProfile("Name", "email@x.com"),
            SelectedFolders = []
        };
        onboarding.CompleteOnboardingAsync(Arg.Any<OneDriveAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveAccount, PersistenceError>>(new Ok<OneDriveAccount, PersistenceError>(account)));
        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenAddAccountWizard.Execute().Subscribe();
        var wizard = (AddAccountWizardViewModel)sut.CurrentOverlay!;

        wizard.SimulateCompleted(account);

        sut.Accounts[^1].AccountId.ShouldBe("acc-wiz");
    }

    [Fact]
    public async Task when_load_persisted_accounts_is_called_then_include_sync_rules_are_loaded_as_folder_nodes()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>(
            [
                new AccountEntity
                {
                    Id = new Persistence.ValueObjects.AccountId("acc-1"),
                    Profile = new AccountProfileEntity { DisplayName = new DisplayName("Alice"), Email = new EmailAddress("alice@x.com") },
                    IsActive = true,
                    DriveId = new DriveId("drive-1"),
                    SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/alice/OneDrive"), WorkerCount = 8 }
                }
            ]));
        var syncRuleRepo = Substitute.For<ISyncRuleRepository>();
        syncRuleRepo.GetAllByAccountIdsAsync(Arg.Any<IEnumerable<Persistence.ValueObjects.AccountId>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyDictionary<Persistence.ValueObjects.AccountId, IReadOnlyList<SyncRule>>>(
                new Dictionary<Persistence.ValueObjects.AccountId, IReadOnlyList<SyncRule>>
                {
                    [new Persistence.ValueObjects.AccountId("acc-1")] = [new SyncRule("/Desktop", RuleType.Include)]
                }));
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo, null, syncRuleRepo);

        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        sut.Accounts[0].Folders.Count.ShouldBe(1);
        sut.Accounts[0].Folders[0].Name.ShouldBe("Desktop");
    }

    [Fact]
    public async Task when_trigger_sync_is_executed_then_scheduler_is_called_for_selected_account()
    {
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>(
            [
                new AccountEntity
                {
                    Id = new Persistence.ValueObjects.AccountId("acc-1"),
                    Profile = new AccountProfileEntity { DisplayName = new DisplayName("Alice"), Email = new EmailAddress("alice@x.com") },
                    IsActive = true,
                    DriveId = new DriveId("drive-1"),
                    SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/alice/OneDrive"), WorkerCount = 8 }
                }
            ]));
        var syncRuleRepo = Substitute.For<ISyncRuleRepository>();
        syncRuleRepo.GetAllByAccountIdsAsync(Arg.Any<IEnumerable<Persistence.ValueObjects.AccountId>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyDictionary<Persistence.ValueObjects.AccountId, IReadOnlyList<SyncRule>>>(
                new Dictionary<Persistence.ValueObjects.AccountId, IReadOnlyList<SyncRule>>()));
        var scheduler = Substitute.For<ISyncScheduler>();
        scheduler.TriggerAccountAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var sut = new WorkspaceViewModel(new ServiceCollection().BuildServiceProvider(), accountRepo, null, syncRuleRepo, scheduler);
        await sut.LoadPersistedAccountsAsync(CancellationToken.None);

        await sut.TriggerSync.Execute().FirstAsync();

        await scheduler.Received(1).TriggerAccountAsync("acc-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void when_open_settings_is_executed_then_current_overlay_is_settings_view_model()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);

        sut.OpenSettings.Execute().Subscribe();

        sut.CurrentOverlay.ShouldNotBeNull();
        sut.CurrentOverlay.ShouldBeOfType<SettingsViewModel>();
    }

    [Fact]
    public void when_settings_closed_event_fires_then_current_overlay_is_null()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenSettings.Execute().Subscribe();

        var settings = (SettingsViewModel)sut.CurrentOverlay!;
        settings.Close.Execute().Subscribe();

        sut.CurrentOverlay.ShouldBeNull();
    }

    [Fact]
    public void when_open_settings_is_executed_then_previous_overlay_is_replaced()
    {
        var auth = Substitute.For<IAuthService>();
        var graph = Substitute.For<IGraphService>();
        var onboarding = Substitute.For<IAccountOnboardingService>();
        var services = new ServiceCollection();
        services.AddTransient(_ => new AddAccountWizardViewModel(auth, graph, onboarding));
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenAddAccountWizard.Execute().Subscribe();

        sut.OpenSettings.Execute().Subscribe();

        sut.CurrentOverlay.ShouldNotBeNull();
        sut.CurrentOverlay.ShouldBeOfType<SettingsViewModel>();
    }

    [Fact]
    public void when_open_settings_executed_twice_then_second_settings_vm_is_the_overlay()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenSettings.Execute().Subscribe();

        sut.OpenSettings.Execute().Subscribe();

        sut.CurrentOverlay.ShouldNotBeNull();
        sut.CurrentOverlay.ShouldBeOfType<SettingsViewModel>();
    }

    [Fact]
    public void when_settings_closed_then_overlay_property_changed_fires()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenSettings.Execute().Subscribe();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        var settings = (SettingsViewModel)sut.CurrentOverlay!;
        settings.Close.Execute().Subscribe();

        raisedProperties.ShouldContain(nameof(WorkspaceViewModel.CurrentOverlay));
    }

    [Fact]
    public void when_settings_is_closed_then_current_overlay_property_changed_fires_exactly_once()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenSettings.Execute().Subscribe();
        var settings = (SettingsViewModel)sut.CurrentOverlay!;
        var changeCount = 0;
        sut.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(WorkspaceViewModel.CurrentOverlay)) changeCount++; };

        settings.Close.Execute().Subscribe();

        changeCount.ShouldBe(1);
    }

    [Fact]
    public void when_settings_close_is_called_again_after_overlay_is_already_null_then_overlay_stays_null()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenSettings.Execute().Subscribe();
        var settings = (SettingsViewModel)sut.CurrentOverlay!;
        settings.Close.Execute().Subscribe();

        settings.Close.Execute().Subscribe();

        sut.CurrentOverlay.ShouldBeNull();
    }

    [Fact]
    public void when_settings_is_reopened_after_close_and_then_closed_again_then_overlay_is_null()
    {
        var services = new ServiceCollection();
        services.AddTransient<SettingsViewModel>();
        var provider = services.BuildServiceProvider();
        var sut = new WorkspaceViewModel(provider);
        sut.OpenSettings.Execute().Subscribe();
        ((SettingsViewModel)sut.CurrentOverlay!).Close.Execute().Subscribe();

        sut.OpenSettings.Execute().Subscribe();
        ((SettingsViewModel)sut.CurrentOverlay!).Close.Execute().Subscribe();

        sut.CurrentOverlay.ShouldBeNull();
    }

}
