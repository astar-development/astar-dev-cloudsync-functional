using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Onboarding;

/// <inheritdoc />
public sealed partial class AccountOnboardingService(ILogger<AccountOnboardingService> logger) : IAccountOnboardingService
{
    /// <inheritdoc />
    public Task<Result<OneDriveAccount, PersistenceError>> CompleteOnboardingAsync(OneDriveAccount account, CancellationToken ct = default)
        => throw new NotImplementedException();
}
