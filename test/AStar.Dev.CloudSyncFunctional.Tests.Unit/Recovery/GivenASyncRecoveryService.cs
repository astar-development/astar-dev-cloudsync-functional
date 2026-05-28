using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Recovery;
using AStar.Dev.FunctionalParadigm;
using FpUnit = AStar.Dev.FunctionalParadigm.Unit;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Recovery;

public class GivenASyncRecoveryService
{
    private static AccountEntity CreateAccountEntity(string id = "acc-1", string name = "Test User", string email = "test@example.com") =>
        new()
        {
            Id = new AccountId(id),
            Profile = new AccountProfileEntity { DisplayName = new DisplayName(name), Email = new EmailAddress(email) },
            IsActive = true,
            DriveId = new DriveId("drive-1"),
            SyncConfig = new AccountSyncConfig { LocalSyncPath = new LocalSyncPath("/home/test/OneDrive"), WorkerCount = 4 }
        };

    private static SyncJobEntity CreateRunningJob(AccountId accountId) =>
        new()
        {
            Id = new SyncJobId(Guid.NewGuid().ToString()),
            AccountId = accountId,
            RemotePath = "/Documents/file.docx",
            LocalPath = "/home/test/OneDrive/Documents/file.docx",
            JobType = "Download",
            Status = "Running",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

    [Fact]
    public async Task when_no_accounts_have_running_jobs_then_detect_returns_empty()
    {
        var accountId = new AccountId("acc-1");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([CreateAccountEntity()]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_account_has_running_jobs_and_delta_link_exists_then_can_resume_is_true()
    {
        var accountId = new AccountId("acc-2");
        var accountEntity = CreateAccountEntity("acc-2", "Alice", "alice@example.com");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([accountEntity]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([CreateRunningJob(accountId)]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        driveStateRepo.GetByAccountAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Option<DriveStateEntity>>(new Some<DriveStateEntity>(new DriveStateEntity
            {
                AccountId = accountId,
                DeltaLink = "https://graph.microsoft.com/v1.0/drives/xxx/root/delta?token=abc",
                LastCheckedAt = DateTimeOffset.UtcNow.AddHours(-1)
            })));
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].CanResume.ShouldBeTrue();
        result[0].AccountId.ShouldBe(accountId);
    }

    [Fact]
    public async Task when_account_has_running_jobs_and_no_delta_link_then_can_resume_is_false()
    {
        var accountId = new AccountId("acc-3");
        var accountEntity = CreateAccountEntity("acc-3", "Bob", "bob@example.com");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([accountEntity]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([CreateRunningJob(accountId)]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        driveStateRepo.GetByAccountAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Option<DriveStateEntity>>(new None<DriveStateEntity>()));
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].CanResume.ShouldBeFalse();
        result[0].AccountId.ShouldBe(accountId);
    }

    [Fact]
    public async Task when_reset_is_called_then_sync_repository_reset_method_is_called()
    {
        var accountId = new AccountId("acc-4");
        var accountRepo = Substitute.For<IAccountRepository>();
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.ResetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<FpUnit, PersistenceError>>(new Ok<FpUnit, PersistenceError>(FpUnit.Default)));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        await sut.ResetAsync(accountId, CancellationToken.None);

        await syncRepo.Received(1).ResetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task when_account_has_running_jobs_then_detect_returns_account_display_name_in_account_name()
    {
        var accountId = new AccountId("acc-5");
        var accountEntity = CreateAccountEntity("acc-5", "Carol Smith", "carol@example.com");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([accountEntity]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([CreateRunningJob(accountId)]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        driveStateRepo.GetByAccountAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Option<DriveStateEntity>>(new None<DriveStateEntity>()));
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result[0].AccountName.ShouldBe("Carol Smith");
    }

    [Fact]
    public async Task when_drive_state_has_empty_delta_link_then_can_resume_is_false()
    {
        var accountId = new AccountId("acc-6");
        var accountEntity = CreateAccountEntity("acc-6", "Dave", "dave@example.com");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([accountEntity]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([CreateRunningJob(accountId)]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        driveStateRepo.GetByAccountAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Option<DriveStateEntity>>(new Some<DriveStateEntity>(new DriveStateEntity
            {
                AccountId = accountId,
                DeltaLink = string.Empty,
                LastCheckedAt = DateTimeOffset.UtcNow.AddHours(-1)
            })));
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result[0].CanResume.ShouldBeFalse();
    }

    [Fact]
    public async Task when_can_resume_is_true_then_message_is_resume_message()
    {
        var accountId = new AccountId("acc-7");
        var accountEntity = CreateAccountEntity("acc-7", "Eve", "eve@example.com");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([accountEntity]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([CreateRunningJob(accountId)]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        driveStateRepo.GetByAccountAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Option<DriveStateEntity>>(new Some<DriveStateEntity>(new DriveStateEntity
            {
                AccountId = accountId,
                DeltaLink = "https://graph.microsoft.com/v1.0/drives/yyy/root/delta?token=xyz",
                LastCheckedAt = DateTimeOffset.UtcNow.AddHours(-2)
            })));
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result[0].Message.ShouldBe("Sync resumed from last checkpoint.");
    }

    [Fact]
    public async Task when_can_resume_is_false_then_message_is_no_checkpoint_message()
    {
        var accountId = new AccountId("acc-8");
        var accountEntity = CreateAccountEntity("acc-8", "Frank", "frank@example.com");
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AccountEntity>>([accountEntity]));
        var syncRepo = Substitute.For<ISyncRepository>();
        syncRepo.GetInterruptedJobsAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SyncJobEntity>>([CreateRunningJob(accountId)]));
        var driveStateRepo = Substitute.For<IDriveStateRepository>();
        driveStateRepo.GetByAccountAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Option<DriveStateEntity>>(new None<DriveStateEntity>()));
        var sut = new SyncRecoveryService(accountRepo, syncRepo, driveStateRepo);

        var result = await sut.DetectAsync(CancellationToken.None);

        result[0].Message.ShouldBe("Sync interrupted. No checkpoint found — a full sync will run on next attempt.");
    }
}
