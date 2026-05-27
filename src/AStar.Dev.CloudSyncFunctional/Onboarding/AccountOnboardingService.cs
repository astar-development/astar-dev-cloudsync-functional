using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Onboarding;

/// <inheritdoc />
public sealed partial class AccountOnboardingService(ILogger<AccountOnboardingService> logger) : IAccountOnboardingService
{
    /// <inheritdoc />
    public Task<Result<OneDriveAccount, PersistenceError>> CompleteOnboardingAsync(OneDriveAccount account, CancellationToken ct = default)
    {
        account.IsActive = true;
        LogOnboardingComplete(logger, account.AccountId);

        return Task.FromResult<Result<OneDriveAccount, PersistenceError>>(new Ok<OneDriveAccount, PersistenceError>(account));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Account onboarding completed for {AccountId}")]
    private static partial void LogOnboardingComplete(ILogger logger, string accountId);
}
