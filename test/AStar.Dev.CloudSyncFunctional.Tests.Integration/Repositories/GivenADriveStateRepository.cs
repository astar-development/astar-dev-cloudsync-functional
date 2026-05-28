using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.Repositories;

public class GivenADriveStateRepository(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private DriveStateRepository CreateSut() => new(new TestDbContextFactory(db.Connection));
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

    private static DriveStateEntity CreateDriveStateEntity(AccountId accountId) =>
        new()
        {
            AccountId = accountId,
            DeltaLink = "https://graph.microsoft.com/v1.0/drives/xxx/root/delta?token=abc",
            LastCheckedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

    [Fact]
    public async Task when_a_drive_state_is_upserted_then_result_is_ok()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var entity = CreateDriveStateEntity(accountId);
        var sut = CreateSut();

        var result = await sut.UpsertAsync(entity, CancellationToken.None);

        result.ShouldBeOfType<Ok<Unit, PersistenceError>>();
    }

    [Fact]
    public async Task when_a_drive_state_is_retrieved_then_it_matches_what_was_stored()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var entity = CreateDriveStateEntity(accountId);
        var sut = CreateSut();

        await sut.UpsertAsync(entity, CancellationToken.None);
        var result = await sut.GetByAccountAsync(accountId, CancellationToken.None);

        var some = (Some<DriveStateEntity>)result;
        some.Value.DeltaLink.ShouldBe(entity.DeltaLink);
    }

    [Fact]
    public async Task when_drive_state_for_non_existent_account_is_retrieved_then_result_is_none()
    {
        var missingId = new AccountId(Guid.NewGuid().ToString());
        var sut = CreateSut();

        var result = await sut.GetByAccountAsync(missingId, CancellationToken.None);

        result.ShouldBeOfType<None<DriveStateEntity>>();
    }
}
