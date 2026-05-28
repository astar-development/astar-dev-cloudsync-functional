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

    private static SyncJobEntity CreateCompletedJob(AccountId accountId) =>
        new()
        {
            Id = new SyncJobId(Guid.NewGuid().ToString()),
            AccountId = accountId,
            RemotePath = "/Documents/done.docx",
            LocalPath = "/home/test/OneDrive/Documents/done.docx",
            JobType = "Download",
            Status = "Completed",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
        };

    private static SyncJobEntity CreatePendingJob(AccountId accountId) =>
        new()
        {
            Id = new SyncJobId(Guid.NewGuid().ToString()),
            AccountId = accountId,
            RemotePath = "/Documents/pending.docx",
            LocalPath = "/home/test/OneDrive/Documents/pending.docx",
            JobType = "Download",
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
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

    [Fact]
    public async Task when_only_completed_jobs_exist_then_get_interrupted_jobs_returns_empty()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var sut = CreateSut();
        await sut.UpsertJobAsync(CreateCompletedJob(accountId), CancellationToken.None);

        var result = await sut.GetInterruptedJobsAsync(accountId, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_only_pending_jobs_exist_then_get_interrupted_jobs_returns_empty()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var sut = CreateSut();
        await sut.UpsertJobAsync(CreatePendingJob(accountId), CancellationToken.None);

        var result = await sut.GetInterruptedJobsAsync(accountId, CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_no_running_jobs_exist_then_reset_interrupted_jobs_returns_ok()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var sut = CreateSut();

        var result = await sut.ResetInterruptedJobsAsync(accountId, CancellationToken.None);

        result.ShouldBeOfType<Ok<Unit, PersistenceError>>();
    }

    [Fact]
    public async Task when_multiple_running_jobs_exist_then_all_are_reset_by_reset_interrupted_jobs()
    {
        var accountId = new AccountId(Guid.NewGuid().ToString());
        await CreateAccountSut().UpsertAsync(CreateAccountEntity(accountId), CancellationToken.None);
        var sut = CreateSut();
        await sut.UpsertJobAsync(CreateRunningJob(accountId), CancellationToken.None);
        await sut.UpsertJobAsync(CreateRunningJob(accountId), CancellationToken.None);
        await sut.UpsertJobAsync(CreateRunningJob(accountId), CancellationToken.None);

        await sut.ResetInterruptedJobsAsync(accountId, CancellationToken.None);
        var remaining = await sut.GetInterruptedJobsAsync(accountId, CancellationToken.None);

        remaining.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_running_jobs_belong_to_different_account_then_get_interrupted_jobs_returns_empty_for_queried_account()
    {
        var ownerAccountId = new AccountId(Guid.NewGuid().ToString());
        var otherAccountId = new AccountId(Guid.NewGuid().ToString());
        var accountSut = CreateAccountSut();
        await accountSut.UpsertAsync(CreateAccountEntity(ownerAccountId), CancellationToken.None);
        await accountSut.UpsertAsync(CreateAccountEntity(otherAccountId), CancellationToken.None);
        var sut = CreateSut();
        await sut.UpsertJobAsync(CreateRunningJob(otherAccountId), CancellationToken.None);

        var result = await sut.GetInterruptedJobsAsync(ownerAccountId, CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
