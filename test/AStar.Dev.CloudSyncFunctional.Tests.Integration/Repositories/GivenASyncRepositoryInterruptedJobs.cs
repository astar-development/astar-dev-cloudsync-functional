using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.Repositories;

public class GivenASyncRepositoryInterruptedJobs(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private SyncRepository CreateSut() => new(new TestDbContextFactory(db.Connection));
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

    private static SyncJobEntity CreateRunningJob(AccountId accountId) =>
        new()
        {
            Id = new SyncJobId(Guid.NewGuid().ToString()),
            AccountId = accountId,
            RemotePath = "/Documents/report.docx",
            LocalPath = "/home/test/OneDrive/Documents/report.docx",
            JobType = "Download",
            Status = "Running",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

    [Fact]
    public async Task when_running_jobs_exist_then_get_interrupted_jobs_returns_them()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var job = CreateRunningJob(accountId);
        var sut = CreateSut();
        await sut.UpsertJobAsync(job, CancellationToken.None);

        var result = await sut.GetInterruptedJobsAsync(accountId, CancellationToken.None);

        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task when_no_running_jobs_exist_then_get_interrupted_jobs_returns_empty()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var sut = CreateSut();

        var result = await sut.GetInterruptedJobsAsync(accountId, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_running_jobs_are_reset_then_status_becomes_interrupted()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var job = CreateRunningJob(accountId);
        var sut = CreateSut();
        await sut.UpsertJobAsync(job, CancellationToken.None);

        var resetResult = await sut.ResetInterruptedJobsAsync(accountId, CancellationToken.None);
        var remainingRunning = await sut.GetInterruptedJobsAsync(accountId, CancellationToken.None);

        resetResult.ShouldBeOfType<Ok<Unit, PersistenceError>>();
        remainingRunning.ShouldBeEmpty();
    }
}
