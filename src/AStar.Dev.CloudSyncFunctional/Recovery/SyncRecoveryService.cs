using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Recovery;

/// <inheritdoc/>
public sealed class SyncRecoveryService(IAccountRepository accountRepository, ISyncRepository syncRepository, IDriveStateRepository driveStateRepository) : ISyncRecoveryService
{
    private const string ResumeMessage = "Sync resumed from last checkpoint.";
    private const string NoCheckpointMessage = "Sync interrupted. No checkpoint found — a full sync will run on next attempt.";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<InterruptedSyncInfo>> DetectAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await accountRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var results = new List<InterruptedSyncInfo>();

        foreach (var account in accounts)
        {
            var interrupted = await syncRepository.GetInterruptedJobsAsync(account.Id, cancellationToken).ConfigureAwait(false);
            if (interrupted.Count == 0)
                continue;

            var driveState = await driveStateRepository.GetByAccountAsync(account.Id, cancellationToken).ConfigureAwait(false);
            var canResume = driveState.Match(
                state => !string.IsNullOrEmpty(state.DeltaLink),
                _ => false);

            results.Add(new InterruptedSyncInfo(account.Id, account.Profile.DisplayName.Value, canResume, canResume ? ResumeMessage : NoCheckpointMessage));
        }

        return results;
    }

    /// <inheritdoc/>
    public Task<Result<Unit, PersistenceError>> ResetAsync(AccountId accountId, CancellationToken cancellationToken = default)
        => syncRepository.ResetInterruptedJobsAsync(accountId, cancellationToken);
}
