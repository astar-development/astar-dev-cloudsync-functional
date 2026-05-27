using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Onboarding;

/// <summary>Persists a new account and its initial sync configuration after the wizard completes.</summary>
public interface IAccountOnboardingService
{
    /// <summary>Completes account onboarding and returns the finalised account.</summary>
    /// <param name="account">The account to onboard.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    /// <returns>The finalised <see cref="OneDriveAccount"/> on success, or a <see cref="PersistenceError"/> on failure.</returns>
    Task<Result<OneDriveAccount, PersistenceError>> CompleteOnboardingAsync(OneDriveAccount account, CancellationToken cancellationToken = default);
}
