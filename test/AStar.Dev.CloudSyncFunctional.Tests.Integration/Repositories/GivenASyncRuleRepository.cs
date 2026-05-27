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
        var entity = CreateEntity(accountId);
        var sut = CreateSut();

        var result = await sut.UpsertAsync(entity, CancellationToken.None);

        result.ShouldBeOfType<Ok<Unit, PersistenceError>>();
    }

    [Fact]
    public async Task when_sync_rules_are_upserted_then_they_can_be_retrieved_by_account()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        var entity = CreateEntity(accountId);
        var sut = CreateSut();

        await sut.UpsertAsync(entity, CancellationToken.None);
        var result = await sut.GetByAccountAsync(accountId, CancellationToken.None);

        result.Count.ShouldBe(1);
    }
}
