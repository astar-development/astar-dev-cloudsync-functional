using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.Repositories;

public class GivenAnAccountRepository(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private AccountRepository CreateSut() => new(new TestDbContextFactory(db.Connection));

    private static AccountEntity CreateEntity() =>
        new()
        {
            Id = new AccountId(Guid.NewGuid().ToString()),
            Profile = new AccountProfileEntity
            {
                DisplayName = new DisplayName("Test User"),
                Email = new EmailAddress("test@example.com")
            },
            IsActive = true,
            DriveId = new DriveId("drive-1"),
            SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/test/OneDrive"), WorkerCount = 4 }
        };

    [Fact]
    public async Task when_an_account_is_upserted_then_result_is_ok()
    {
        var entity = CreateEntity();
        var sut = CreateSut();

        var result = await sut.UpsertAsync(entity, CancellationToken.None);

        result.ShouldBeOfType<Ok<Unit, PersistenceError>>();
    }

    [Fact]
    public async Task when_an_account_is_upserted_then_it_can_be_retrieved_by_id()
    {
        var entity = CreateEntity();
        var sut = CreateSut();

        await sut.UpsertAsync(entity, CancellationToken.None);
        var result = await sut.GetByIdAsync(entity.Id, CancellationToken.None);

        result.ShouldBeOfType<Some<AccountEntity, PersistenceError>>();
    }

    [Fact]
    public async Task when_a_non_existent_account_is_retrieved_then_result_is_none()
    {
        var sut = CreateSut();
        var missingId = new AccountId(Guid.NewGuid().ToString());

        var result = await sut.GetByIdAsync(missingId, CancellationToken.None);

        result.ShouldBeOfType<None<AccountEntity, PersistenceError>>();
    }

    [Fact]
    public async Task when_an_account_is_deleted_then_get_by_id_returns_none()
    {
        var entity = CreateEntity();
        var sut = CreateSut();

        await sut.UpsertAsync(entity, CancellationToken.None);
        await sut.DeleteAsync(entity.Id, CancellationToken.None);
        var result = await sut.GetByIdAsync(entity.Id, CancellationToken.None);

        result.ShouldBeOfType<None<AccountEntity, PersistenceError>>();
    }
}
