using System.Collections.Concurrent;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Drives periodic and on-demand sync scheduling for all active accounts.</summary>
public sealed partial class SyncScheduler(ISyncService syncService, IAccountRepository accountRepository, ILogger<SyncScheduler> logger) : ISyncScheduler, IAsyncDisposable
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(60);

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeSyncs = new();
    private System.Threading.Timer? _timer;
    private TimeSpan _interval = DefaultInterval;
    private long _runningFlag;

    /// <inheritdoc />
    public event EventHandler<string>? SyncStarted;

    /// <inheritdoc />
    public event EventHandler<string>? SyncCompleted;

    /// <inheritdoc />
    public void StartSync(TimeSpan? interval = null)
    {
        _interval = interval ?? DefaultInterval;
        _timer?.Dispose();
        _timer = new System.Threading.Timer(OnTimerTickAsync, null, _interval, _interval);
        LogSchedulerStarted(logger, (long)_interval.TotalMinutes);
    }

    /// <inheritdoc />
    public void StopSync()
    {
        _timer?.Change(System.Threading.Timeout.InfiniteTimeSpan, System.Threading.Timeout.InfiniteTimeSpan);
        LogSchedulerStopped(logger);
    }

    /// <inheritdoc />
    public void SetInterval(TimeSpan interval)
    {
        _interval = interval;
        _timer?.Change(interval, interval);
    }

    /// <inheritdoc />
    public async Task TriggerNowAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Read(ref _runningFlag) == 1)
            return;

        await RunSyncPassAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task TriggerAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var accounts = await accountRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var entity = accounts.FirstOrDefault(account => account.Id.Value == accountId);
        if (entity is null)
            return;

        var domainAccount = new OneDriveAccount
        {
            AccountId = Auth.AccountId.Create(entity.Id.Value),
            IsActive = entity.IsActive,
            DriveId = entity.DriveId.Value,
            DriveIdValue = new Persistence.ValueObjects.DriveId(entity.DriveId.Value),
            SyncConfig = entity.SyncConfig
        };

        await TriggerAccountAsync(domainAccount, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task TriggerAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _activeSyncs[account.AccountId.Value] = cts;

        try
        {
            SyncStarted?.Invoke(this, account.AccountId.Value);
            await syncService.SyncAccountAsync(account, cts.Token).ConfigureAwait(false);
            SyncCompleted?.Invoke(this, account.AccountId.Value);
        }
        catch (OperationCanceledException)
        {
            LogSyncCancelled(logger, account.AccountId.Value);
        }
        catch (Exception ex)
        {
            LogSyncException(logger, account.AccountId.Value, ex.Message);
        }
        finally
        {
            _activeSyncs.TryRemove(account.AccountId.Value, out _);
            cts.Dispose();
        }
    }

    /// <inheritdoc />
    public Task CancelAccountSyncAsync(string accountId)
    {
        if (_activeSyncs.TryGetValue(accountId, out var cts))
            cts.Cancel();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        StopSync();
        foreach (var cts in _activeSyncs.Values)
            cts.Cancel();

        _activeSyncs.Clear();
        if (_timer is not null)
            await _timer.DisposeAsync().ConfigureAwait(false);
    }

    // ReSharper disable once AsyncVoidMethod - Timer requires this signature
    private async void OnTimerTickAsync(object? state)
    {
        try
        {
            await RunSyncPassAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogTimerTickException(logger, ex.Message);
        }
    }

    private async Task RunSyncPassAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _runningFlag, 1) == 1)
            return;

        try
        {
            var accounts = await accountRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            foreach (var entity in accounts.Where(account => account.IsActive))
            {
                var domainAccount = new OneDriveAccount
                {
                    AccountId = Auth.AccountId.Create(entity.Id.Value),
                    IsActive = entity.IsActive,
                    DriveId = entity.DriveId.Value,
                    DriveIdValue = new Persistence.ValueObjects.DriveId(entity.DriveId.Value),
                    SyncConfig = entity.SyncConfig
                };

                await TriggerAccountAsync(domainAccount, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _runningFlag, 0);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sync scheduler started with interval {IntervalMinutes} minutes")]
    private static partial void LogSchedulerStarted(ILogger logger, long intervalMinutes);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sync scheduler stopped")]
    private static partial void LogSchedulerStopped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sync cancelled for account {AccountId}")]
    private static partial void LogSyncCancelled(ILogger logger, string accountId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Sync exception for account {AccountId}: {ErrorMessage}")]
    private static partial void LogSyncException(ILogger logger, string accountId, string errorMessage);

    [LoggerMessage(Level = LogLevel.Error, Message = "Timer tick exception: {ErrorMessage}")]
    private static partial void LogTimerTickException(ILogger logger, string errorMessage);
}
