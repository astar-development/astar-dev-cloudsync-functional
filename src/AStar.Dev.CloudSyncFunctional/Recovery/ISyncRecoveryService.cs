using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Recovery;

/// <summary>Detects and resets interrupted syncs on application startup.</summary>
public interface ISyncRecoveryService
{
    /// <summary>Scans all active accounts for jobs that were in-flight when the app crashed.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recovery info per interrupted account.</returns>
    Task<IReadOnlyList<InterruptedSyncInfo>> DetectAsync(CancellationToken cancellationToken = default);

    /// <summary>Resets interrupted jobs for a specific account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success, Fail on persistence error.</returns>
    Task<Result<Unit, PersistenceError>> ResetAsync(AccountId accountId, CancellationToken cancellationToken = default);
}
