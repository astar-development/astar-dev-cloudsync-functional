using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.Repositories;

public class GivenASyncRuleRepository(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private SyncRuleRepository CreateSut() => new(new TestDbContextFactory(db.Connection));
    private AccountRepository CreateAccountSut() => new(new TestDbContextFactory(db.Connection));

    private static AccountEntity CreateAccountEntity(AccountId accountId) =>
        new()
        {
            Id = accountId,
            Profile = new AccountProfileEntity
            {
                DisplayName = new DisplayName("Test User"),
                Email = new EmailAddress("test@example.com")
            },
            IsActive = true,
            DriveId = new DriveId("drive-1"),
            SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/test/OneDrive"), WorkerCount = 4 }
        };

    private static SyncRuleEntity CreateEntity(AccountId accountId) =>
        new()
        {
            Id = new SyncRuleId(Guid.NewGuid().ToString()),
            AccountId = accountId,
            RemotePath = "/Documents",
            RuleType = RuleType.Include
        };

    [Fact]
    public async Task when_a_sync_rule_is_upserted_then_result_is_ok()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var entity = CreateEntity(accountId);
        var sut = CreateSut();

        var result = await sut.UpsertAsync(entity, CancellationToken.None);

        result.ShouldBeOfType<Ok<Unit, PersistenceError>>();
    }

    [Fact]
    public async Task when_sync_rules_are_upserted_then_they_can_be_retrieved_by_account()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var entity = CreateEntity(accountId);
        var sut = CreateSut();

        await sut.UpsertAsync(entity, CancellationToken.None);
        var result = await sut.GetByAccountAsync(accountId, CancellationToken.None);

        result.Count.ShouldBe(1);
    }

    [Fact]
    public async Task when_sync_rules_for_different_accounts_do_not_overlap()
    {
        var accountIdA = new AccountId(Guid.NewGuid().ToString());
        var accountIdB = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountIdA), CancellationToken.None);
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountIdB), CancellationToken.None);
        var sut = CreateSut();

        await sut.UpsertAsync(CreateEntity(accountIdA), CancellationToken.None);
        await sut.UpsertAsync(CreateEntity(accountIdB), CancellationToken.None);
        var rulesForA = await sut.GetByAccountAsync(accountIdA, CancellationToken.None);

        rulesForA.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task when_a_sync_rule_is_deleted_then_it_cannot_be_retrieved()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var entity = CreateEntity(accountId);
        var sut = CreateSut();

        await sut.UpsertAsync(entity, CancellationToken.None);
        await sut.DeleteAsync(entity.Id, CancellationToken.None);
        var result = await sut.GetByAccountAsync(accountId, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_deleting_a_non_existent_sync_rule_then_result_is_ok()
    {
        var missingId = new SyncRuleId(Guid.NewGuid().ToString());
        var sut = CreateSut();

        var result = await sut.DeleteAsync(missingId, CancellationToken.None);

        result.ShouldBeOfType<Ok<Unit, PersistenceError>>();
    }
}
