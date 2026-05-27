# Background Work — Sync Scheduler and Async Patterns

## Sync scheduler

`SyncScheduler` drives periodic sync using a `System.Threading.Timer`. Default interval: **60 minutes**.

### Start / stop / manual trigger

```csharp
scheduler.StartSync();                    // begin periodic sync
scheduler.StartSync(TimeSpan.FromMinutes(30));  // custom interval
scheduler.StopSync();                     // pause without disposing
scheduler.SetInterval(newInterval);       // change interval mid-run

await scheduler.TriggerNowAsync(ct);     // immediate one-shot sync (all accounts)
await scheduler.TriggerAccountAsync(accountId, ct);  // sync a single account
await scheduler.CancelAccountSyncAsync(accountId);   // cancel in-flight sync for account
```

### Re-entrancy guard

Use `Interlocked.Exchange` to prevent concurrent sync passes:

```csharp
private long _runningFlag;

private async Task RunSyncPassAsync(CancellationToken cancellationToken)
{
    if (Interlocked.Exchange(ref _runningFlag, 1) == 1) return;
    try { /* sync all accounts */ }
    finally { Interlocked.Exchange(ref _runningFlag, 0); }
}
```

`TriggerNowAsync` also guards with `Interlocked.Read` before calling `RunSyncPassAsync`.

### Per-account cancellation

Active syncs are tracked in a `ConcurrentDictionary<string, CancellationTokenSource>` keyed on `accountId`. `CancelAccountSyncAsync` calls `cts.Cancel()` on the matching entry. The entry is removed in the `finally` block of `TriggerAccountAsync`.

### Timer callback signature

`Timer` requires `async void`. This is the **only** place `async void` is permitted:

```csharp
// ReSharper disable once AsyncVoidMethod - Timer requires this signature
private async void OnTimerTickAsync(object? state)
{
    try { await RunSyncPassAsync(CancellationToken.None); }
    catch (Exception ex) { /* log */ }
}
```

Wrap the body in `try/catch Exception` — unhandled exceptions in `async void` crash the process.

### Disposal

`SyncScheduler` implements `IAsyncDisposable`:

```csharp
public async ValueTask DisposeAsync()
{
    StopSync();
    foreach (var cts in _activeSyncs.Values) cts.Cancel();
    _activeSyncs.Clear();
    if (_timer is not null) await _timer.DisposeAsync();
}
```

## ISyncScheduler contract

```csharp
event EventHandler<string>? SyncStarted;
event EventHandler<string>? SyncCompleted;

void StartSync(TimeSpan? interval = null);
void StopSync();
void SetInterval(TimeSpan interval);
Task TriggerNowAsync(CancellationToken cancellationToken = default);
Task TriggerAccountAsync(string accountId, CancellationToken cancellationToken = default);
Task TriggerAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default);
Task CancelAccountSyncAsync(string accountId);
```

## CancellationToken rules

- **Every** public async method takes `CancellationToken cancellationToken = default` as the final parameter.
- Propagate `ct` to every downstream `await` — never pass `CancellationToken.None` unless you are deliberately starting unlinked work (e.g. timer-tick root, post-cancellation cleanup).
- Catch `OperationCanceledException` at the service boundary only — do not swallow it inside pipeline steps.
- Use `CancellationTokenSource.CreateLinkedTokenSource(ct)` when you need per-account cancellation composable with an outer token.

## Progress reporting

Never poll for sync status. Report progress via events:

- `ISyncService.SyncProgressChanged` — raised at each pipeline stage and per-file.
- `ISyncService.JobCompleted` — raised after each individual download or upload job finishes.
- `ISyncScheduler.SyncStarted` / `SyncCompleted` — raised per-account at scheduler level.
- `ISyncService.ConflictDetected` — raised when a conflict is found during sync.

Payloads:

```csharp
record SyncProgressEventArgs(string AccountId, string CurrentFile, int Completed, int Total, string StatusMessage, SyncState State);
record JobCompletedEventArgs(string AccountId, string RemotePath, bool Success, string? ErrorMessage);
```

ViewModels subscribe to these events and update observable properties. They never call `Thread.Sleep`, `Task.Delay`, or poll a flag.

## Thread safety

- Event handlers on `ISyncService` and `ISyncScheduler` may be invoked from the thread pool. ViewModels must marshal to the UI thread before updating observable properties:

```csharp
// ReactiveUI — post to UI thread
RxApp.MainThreadScheduler.Schedule(() => UpdateFromProgress(args));
```

- `SyncScheduler._activeSyncs` is a `ConcurrentDictionary` — safe to read/write from multiple threads.
- `SyncScheduler._runningFlag` uses `Interlocked` — no lock required.

## Logging in background services

Use `ILogger<T>` via DI with source-generated log messages (`[LoggerMessage]` attribute). Logging is mandatory — it is never optional and is always added as needed.

```csharp
[LoggerMessage(Level = LogLevel.Debug, Message = "Sync started for account {AccountId}")]
static partial void LogSyncStarted(ILogger logger, string accountId);

[LoggerMessage(Level = LogLevel.Error, Message = "Sync failed for account {AccountId}: {ErrorMessage}")]
static partial void LogSyncFailed(ILogger logger, string accountId, string errorMessage);
```

Log at `Debug` for per-file progress. `Information` for sync start/complete. `Warning` for recoverable errors (access denied, network retry). `Error` for failures that stop the sync.

### Parameter design

Design log method parameters to avoid cardinality explosion. Each distinct message template is treated as a separate event by log analysis tools — hundreds of near-identical templates with embedded values are a defect.

- **Parameters carry the variability** — the template is fixed; low-cardinality identifiers (`accountId`, `itemId`, operation names) go in parameters.
- **Never embed unbounded values** (full file paths, URLs, arbitrary strings) directly in the template string — put them in a parameter.
- **Group related operations** into one log method with a discriminating parameter rather than many near-identical methods.

```csharp
// ❌ cardinality explosion — one template per operation name baked in
[LoggerMessage(Level = LogLevel.Debug, Message = "Starting download for account abc123 file /Documents/Report.docx")]

// ✅ one template, parameters carry the variability
[LoggerMessage(Level = LogLevel.Debug, Message = "Starting {Operation} for account {AccountId}, item {ItemId}")]
static partial void LogOperationStarted(ILogger logger, string accountId, string itemId, string operation);
```

## Application initializer

On startup, an `ApplicationInitializer` (or `IHostedService`) must:

1. Apply pending EF Core migrations (`context.Database.MigrateAsync(ct)`).
2. Load persisted accounts and restore `OneDriveAccount` in-memory state.
3. Re-acquire tokens silently for any persisted account (`AcquireTokenSilentAsync`).
4. Start the `SyncScheduler`.

Run these steps sequentially — the scheduler must not start before migrations complete.
