using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.Onboarding;

public class GivenAnAccountOnboardingServiceIntegration(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private AccountOnboardingService CreateSut() =>
        new(new AccountRepository(new TestDbContextFactory(db.Connection)),
            new SyncRuleRepository(new TestDbContextFactory(db.Connection)),
            Substitute.For<ILogger<AccountOnboardingService>>());

    private SyncRuleRepository CreateSyncRuleRepo() =>
        new(new TestDbContextFactory(db.Connection));

    private AccountRepository CreateAccountRepo() =>
        new(new TestDbContextFactory(db.Connection));

    private static OneDriveAccount CreateAccount(params string[] folderNames) =>
        new()
        {
            AccountId = Guid.NewGuid().ToString(),
            Profile = new AccountProfile("Test User", "test@example.com"),
            SelectedFolders = folderNames.Select((name, i) => new SelectedFolder($"graph-id-{i}", name)).ToList()
        };

    [Fact]
    public async Task when_complete_onboarding_is_called_then_result_is_ok()
    {
        var account = CreateAccount("folder-1", "folder-2");
        var sut = CreateSut();

        var result = await sut.CompleteOnboardingAsync(account, CancellationToken.None);

        result.ShouldBeOfType<Ok<OneDriveAccount, PersistenceError>>();
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_account_is_persisted()
    {
        var account = CreateAccount("folder-1");
        var sut = CreateSut();

        await sut.CompleteOnboardingAsync(account, CancellationToken.None);
        var stored = await CreateAccountRepo().GetByIdAsync(new AccountId(account.AccountId), CancellationToken.None);

        stored.ShouldBeOfType<Some<Persistence.Entities.AccountEntity, PersistenceError>>();
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_sync_rules_are_persisted_for_each_folder()
    {
        var account = CreateAccount("folder-1", "folder-2");
        var sut = CreateSut();

        await sut.CompleteOnboardingAsync(account, CancellationToken.None);
        var rules = await CreateSyncRuleRepo().GetByAccountAsync(new AccountId(account.AccountId), CancellationToken.None);

        rules.Count.ShouldBe(2);
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_with_no_folders_then_no_sync_rules_are_created()
    {
        var account = CreateAccount();
        var sut = CreateSut();

        await sut.CompleteOnboardingAsync(account, CancellationToken.None);
        var rules = await CreateSyncRuleRepo().GetByAccountAsync(new AccountId(account.AccountId), CancellationToken.None);

        rules.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_sync_rule_remote_path_is_slash_prefixed_folder_name()
    {
        var account = new OneDriveAccount
        {
            AccountId = Guid.NewGuid().ToString(),
            Profile = new AccountProfile("Test User", "test@example.com"),
            SelectedFolders = [new SelectedFolder("graph-item-id-abc123", "Documents")]
        };
        var sut = CreateSut();

        await sut.CompleteOnboardingAsync(account, CancellationToken.None);
        var rules = await CreateSyncRuleRepo().GetByAccountAsync(new AccountId(account.AccountId), CancellationToken.None);

        rules[0].RemotePath.ShouldBe("/Documents");
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_sync_rule_remote_path_does_not_contain_graph_item_id()
    {
        var account = new OneDriveAccount
        {
            AccountId = Guid.NewGuid().ToString(),
            Profile = new AccountProfile("Test User", "test@example.com"),
            SelectedFolders = [new SelectedFolder("graph-item-id-abc123", "Pictures")]
        };
        var sut = CreateSut();

        await sut.CompleteOnboardingAsync(account, CancellationToken.None);
        var rules = await CreateSyncRuleRepo().GetByAccountAsync(new AccountId(account.AccountId), CancellationToken.None);

        rules[0].RemotePath.ShouldNotContain("graph-item-id-abc123");
    }
}
