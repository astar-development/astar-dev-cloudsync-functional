using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace AStar.Dev.CloudSyncFunctional.Onboarding;

/// <inheritdoc />
public sealed partial class AccountOnboardingService(IAccountRepository accountRepository, ISyncRuleRepository syncRuleRepository, IFileSystem fileSystem, ILogger<AccountOnboardingService> logger) : IAccountOnboardingService
{
    private static readonly char[] InvalidPathChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|',
        ..Enumerable.Range(0, 32).Select(i => (char)i)];

    /// <inheritdoc />
    public async Task<Result<OneDriveAccount, PersistenceError>> CompleteOnboardingAsync(OneDriveAccount account, CancellationToken cancellationToken = default)
    {
        account.IsActive = true;
        var entity = MapToEntity(account);

        return await accountRepository.UpsertAsync(entity, cancellationToken)
            .BindAsync(_ => UpsertSyncRulesAsync(account, cancellationToken))
            .MatchAsync<Unit, PersistenceError, Result<OneDriveAccount, PersistenceError>>(
                _ =>
                {
                    LogOnboardingComplete(logger, account.AccountId.Value);
                    return new Ok<OneDriveAccount, PersistenceError>(account);
                },
                error =>
                {
                    LogOnboardingFailed(logger, account.AccountId.Value, error.Message);
                    return new Fail<OneDriveAccount, PersistenceError>(error);
                });
    }

    private Task<Result<Unit, PersistenceError>> UpsertSyncRulesAsync(OneDriveAccount account, CancellationToken cancellationToken) =>
        account.SelectedFolders.Aggregate(
            Task.FromResult<Result<Unit, PersistenceError>>(new Ok<Unit, PersistenceError>(Unit.Default)),
            (current, folder) => current.BindAsync(_ => syncRuleRepository.UpsertAsync(CreateSyncRule(account, folder), cancellationToken)));

    private static SyncRuleEntity CreateSyncRule(OneDriveAccount account, SelectedFolder folder) =>
        new()
        {
            Id = new SyncRuleId(Guid.NewGuid().ToString()),
            AccountId = new AccountId(account.AccountId.Value),
            RemotePath = $"/{folder.Name}",
            RuleType = RuleType.Include
        };

    private AccountEntity MapToEntity(OneDriveAccount account) =>
        new()
        {
            Id = new AccountId(account.AccountId.Value),
            Profile = new AccountProfileEntity
            {
                DisplayName = new DisplayName(account.Profile.DisplayName),
                Email = new EmailAddress(account.Profile.Email)
            },
            IsActive = account.IsActive,
            DriveId = new DriveId(account.DriveId ?? string.Empty),
            SyncConfig = new AccountSyncConfig
            {
                LocalSyncPath = new LocalSyncPath(ComputeDefaultSyncPath(account.Profile.Email)),
                WorkerCount = 8
            }
        };

    private string ComputeDefaultSyncPath(string email)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var sanitised = SanitiseEmail(email);

        return fileSystem.Path.Combine(home, "OneDrive", sanitised);
    }

    private static string SanitiseEmail(string email) =>
        string.Concat(email.Where(c => !InvalidPathChars.Contains(c)));

    [LoggerMessage(Level = LogLevel.Information, Message = "Account onboarding completed for {AccountId}")]
    private static partial void LogOnboardingComplete(ILogger logger, string accountId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Account onboarding failed for {AccountId}: {ErrorMessage}")]
    private static partial void LogOnboardingFailed(ILogger logger, string accountId, string errorMessage);
}
