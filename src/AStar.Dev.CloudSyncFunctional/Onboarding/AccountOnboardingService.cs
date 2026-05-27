using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Onboarding;

/// <inheritdoc />
public sealed partial class AccountOnboardingService(IAccountRepository accountRepository, ISyncRuleRepository syncRuleRepository, ILogger<AccountOnboardingService> logger) : IAccountOnboardingService
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
                    LogOnboardingComplete(logger, account.AccountId);
                    return new Ok<OneDriveAccount, PersistenceError>(account);
                },
                error =>
                {
                    LogOnboardingFailed(logger, account.AccountId, error.Message);
                    return new Fail<OneDriveAccount, PersistenceError>(error);
                });
    }

    private async Task<Result<Unit, PersistenceError>> UpsertSyncRulesAsync(OneDriveAccount account, CancellationToken cancellationToken)
    {
        foreach (var folder in account.SelectedFolders)
        {
            var rule = new SyncRuleEntity
            {
                Id = new SyncRuleId(Guid.NewGuid().ToString()),
                AccountId = new AccountId(account.AccountId),
                RemotePath = $"/{folder.Name}",
                RuleType = RuleType.Include
            };

            var stopResult = await syncRuleRepository.UpsertAsync(rule, cancellationToken)
                .MatchAsync<Unit, PersistenceError, Result<Unit, PersistenceError>?>(
                    _ => null,
                    error => new Fail<Unit, PersistenceError>(error))
                .ConfigureAwait(false);

            if (stopResult is not null)
                return stopResult;
        }

        return new Ok<Unit, PersistenceError>(Unit.Default);
    }

    private static AccountEntity MapToEntity(OneDriveAccount account) =>
        new()
        {
            Id = new AccountId(account.AccountId),
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

    private static string ComputeDefaultSyncPath(string email)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var sanitised = SanitiseEmail(email);

        return Path.Combine(home, "OneDrive", sanitised);
    }

    private static string SanitiseEmail(string email) =>
        string.Concat(email.Where(c => !InvalidPathChars.Contains(c)));

    [LoggerMessage(Level = LogLevel.Information, Message = "Account onboarding completed for {AccountId}")]
    private static partial void LogOnboardingComplete(ILogger logger, string accountId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Account onboarding failed for {AccountId}: {ErrorMessage}")]
    private static partial void LogOnboardingFailed(ILogger logger, string accountId, string errorMessage);
}
