using AStar.Dev.CloudSyncFunctional.Domain;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Drives periodic and on-demand sync scheduling across all active accounts.</summary>
public interface ISyncScheduler
{
    /// <summary>Raised when a sync pass begins for a given account; the event argument is the account identifier.</summary>
    event EventHandler<string>? SyncStarted;

    /// <summary>Raised when a sync pass completes for a given account; the event argument is the account identifier.</summary>
    event EventHandler<string>? SyncCompleted;

    /// <summary>Starts the periodic sync timer.</summary>
    /// <param name="interval">The interval between sync passes; defaults to 60 minutes when <c>null</c>.</param>
    void StartSync(TimeSpan? interval = null);

    /// <summary>Stops the periodic sync timer without disposing resources.</summary>
    void StopSync();

    /// <summary>Changes the interval used by the periodic timer.</summary>
    /// <param name="interval">The new interval.</param>
    void SetInterval(TimeSpan interval);

    /// <summary>Triggers an immediate sync pass for all active accounts.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the sync pass finishes.</returns>
    Task TriggerNowAsync(CancellationToken cancellationToken = default);

    /// <summary>Triggers an immediate sync pass for the given account identifier.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the sync pass finishes.</returns>
    Task TriggerAccountAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>Triggers an immediate sync pass for the given account.</summary>
    /// <param name="account">The account to sync.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the sync pass finishes.</returns>
    Task TriggerAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default);

    /// <summary>Cancels any in-flight sync for the given account.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <returns>A task that completes when the cancellation signal has been sent.</returns>
    Task CancelAccountSyncAsync(string accountId);
}
