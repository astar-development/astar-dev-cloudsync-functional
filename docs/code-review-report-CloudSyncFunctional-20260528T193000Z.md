# Code Review — AStar.Dev.CloudSyncFunctional (uncommitted changes)

**Branch:** `feature/27-sync-pipeline`
**Date:** 2026-05-28
**Reviewer:** Claude Code (senior C# / .NET engineer)
**Files reviewed:**
- `src/AStar.Dev.CloudSyncFunctional/Accounts/AccountViewModel.cs`
- `src/AStar.Dev.CloudSyncFunctional/Controls/AccountHeader.axaml`
- `src/AStar.Dev.CloudSyncFunctional/Controls/AccountHeader.axaml.cs`
- `src/AStar.Dev.CloudSyncFunctional/Controls/FolderToolbar.axaml.cs`
- `src/AStar.Dev.CloudSyncFunctional/Workspace/WorkspaceViewModel.cs`
- `src/AStar.Dev.CloudSyncFunctional/MainWindow.axaml`

---

## Findings

### `Workspace/WorkspaceViewModel.cs`

**[error] L5, L188–207 — Persistence entity leaks into the ViewModel layer**

`WorkspaceViewModel` imports `AStar.Dev.CloudSyncFunctional.Persistence.Entities` and `MapToViewModel` accepts `IReadOnlyList<SyncRuleEntity>` directly. This violates the [domain model isolation rule](../.claude/rules/onedrive-viewmodels.md#domain-model-isolation): `*Entity` types must never appear above the service layer. `ISyncRuleRepository.GetByAccountAsync` returns `IReadOnlyList<SyncRuleEntity>` — this means the entity is traversing the full stack into the ViewModel unimpeded.

Fix: introduce a domain record (e.g. `SyncRule`) and map `SyncRuleEntity` → `SyncRule` at the repository or service boundary. `WorkspaceViewModel` should receive `IReadOnlyList<SyncRule>`, not entities.

```csharp
// domain record (co-locate with ISyncRuleRepository)
public sealed record SyncRule(string RemotePath, RuleType RuleType);

// repository maps internally
public async Task<IReadOnlyList<SyncRule>> GetByAccountAsync(AccountId accountId, CancellationToken ct)
{
    await using var context = await _dbFactory.CreateDbContextAsync(ct);
    var rows = await context.SyncRules
        .Where(r => r.AccountId == accountId)
        .ToListAsync(ct);

    return rows.Select(r => new SyncRule(r.RemotePath, r.RuleType)).ToList();
}
```

---

**[error] L108–109 and L146–147 — `TriggerSync` unhandled exceptions, missing error surface**

`ReactiveCommand.CreateFromTask` routes exceptions into `ThrownExceptions`. Neither constructor subscribes to `TriggerSync.ThrownExceptions`. Per the [functional usage rules](../.claude/rules/functional-usage.md#non-negotiable-rules) (rule 7), silent swallowing is banned: every failure must be logged and surfaced to the UI. Additionally, per the [ViewModel rules](../.claude/rules/onedrive-viewmodels.md), every ViewModel that can fail must expose `bool HasError` and `string ErrorMessage`.

Fix:

```csharp
// Add reactive properties
public bool HasError
{
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
}

public string ErrorMessage
{
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
}

// Subscribe in InitializeCommands() (extracted — see next finding)
TriggerSync.ThrownExceptions
    .Subscribe(ex =>
    {
        HasError = true;
        ErrorMessage = ex.Message;
        _logger.LogError(ex, "TriggerSync failed for account {AccountId}", SelectedAccount?.AccountId);
    })
    .DisposeWith(_disposables);
```

---

**[error] L108–109 / L146–147 — `canSync` does not guard against null `_syncScheduler`**

The `canSync` observable only checks `SelectedAccount is not null && AccountId is not empty`. When `_syncScheduler` is `null` (scheduler not registered, mis-wired DI), the Sync Now button is **enabled** for any real account but `ExecuteTriggerSyncAsync` silently returns `Task.CompletedTask`. The user gets no feedback.

Fix: include the scheduler in `canSync`:

```csharp
var canSync = this.WhenAnyValue(
    x => x.SelectedAccount,
    a => a is not null && !string.IsNullOrEmpty(a.AccountId) && _syncScheduler is not null);
```

---

**[warning] L120–126 — N+1 query in `LoadPersistedAccountsAsync`**

One `GetByAccountAsync` call per account in a sequential loop. With N accounts this is N+1 round-trips to SQLite. Acceptable for 1–3 accounts today; becomes a startup bottleneck when account count grows.

Fix: add `GetAllWithRulesAsync()` to the repository (or a `GetAllByAccountIdsAsync(IEnumerable<AccountId>)` batch overload) and do a single query with a join.

Reference: [N+1 detection — Microsoft EF Core performance docs](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying#beware-of-lazy-loading)

---

**[warning] L108–109, L146–147 — `canSync` / `TriggerSync` init duplicated across two constructors**

Both the runtime constructor (L108–109) and the design-time constructor (L146–147) repeat the same two lines. A future change to either will silently miss the other.

Fix: extract to a private method called from both constructors:

```csharp
private CompositeDisposable _disposables = new();

private void InitializeCommands()
{
    var canSync = this.WhenAnyValue(
        x => x.SelectedAccount,
        a => a is not null && !string.IsNullOrEmpty(a.AccountId) && _syncScheduler is not null);

    TriggerSync = ReactiveCommand.CreateFromTask(ExecuteTriggerSyncAsync, canSync);

    TriggerSync.ThrownExceptions
        .Subscribe(ex => { HasError = true; ErrorMessage = ex.Message; })
        .DisposeWith(_disposables);
}
```

---

**[warning] L196–206 — `MapToViewModel` iterates `syncRules` twice**

`syncRules.Count(r => r.RuleType == RuleType.Include)` enumerates the list, then `syncRules.Where(r => r.RuleType == RuleType.Include).Select(...)` enumerates it again.

Fix: single pass:

```csharp
var includedFolders = syncRules
    .Where(r => r.RuleType == RuleType.Include)
    .Select(r => new FolderNode
    {
        Path = r.RemotePath,
        Name = r.RemotePath.TrimStart('/'),
        Depth = 0,
        SelectionState = CheckState.On,
        LastSync = DateTimeOffset.MinValue
    })
    .ToList();

return new AccountViewModel
{
    ...
    FolderCount = includedFolders.Count,
    Folders = [.. includedFolders]
};
```

---

**[warning] L209–214 — `ExecuteTriggerSyncAsync` guard is redundant but hides the real problem**

The null-guard inside the method duplicates the `canSync` predicate, which is fine defensively. However, if `_syncScheduler is null` and `canSync` does not guard for it (see error finding above), this guard silently swallows the no-op with zero user feedback. The guard is load-bearing for hiding the DI misconfiguration bug.

Once `canSync` is fixed to include `_syncScheduler is not null`, the guard can be simplified:

```csharp
private Task ExecuteTriggerSyncAsync(CancellationToken cancellationToken)
    => _syncScheduler!.TriggerAccountAsync(SelectedAccount!.AccountId, cancellationToken);
```

---

### `Controls/AccountHeader.axaml.cs`

**[suggestion] L141 — `OnPropertyChanged` uses multiple `if` (not `else if`) chains**

All property-changed checks fire on every property change, even for properties that are clearly unrelated. Not a bug — Avalonia's `StyledProperty` system ensures the property reference check is fast — but it reads as if each branch is independent when they should be exclusive.

No immediate action required; note it for when the control grows.

---

### `Controls/FolderToolbar.axaml.cs`

**[suggestion] L12 — Default `BreadcrumbPath` changed from `"~/AStar /"` to `string.Empty`**

Clean-up. Correct. No issues.

---

### `Accounts/AccountViewModel.cs`

**[suggestion] L14 — `AccountId` is `string`, not `AccountId` wrapper**

Per [persistence rules](../.claude/rules/onedrive-persistence.md#strongly-typed-domain-value-types), MSAL account identifiers must use the `AccountId` wrapper (`readonly record struct AccountId(string Value)`). ViewModels may hold primitives, but the intent of `AccountId` is to carry domain meaning and avoid mixing it with display strings (`Name`, `Email`).

Borderline acceptable in a ViewModel per the "primitive types" allowance, but using the wrapper would prevent accidentally passing `Email` where `AccountId` is expected. Consider `AccountId AccountId { get; init; }` with `AccountId.Empty` as the default (or just a no-arg `new AccountId(string.Empty)`).

---

## Summary

| Severity | Count |
|---|---|
| 🔴 error | 3 |
| 🟡 warning | 4 |
| 🔵 suggestion | 3 |

## Verdict: **Request Changes**

Three errors block merge:

1. `*Entity` leaking into the ViewModel — architecture violation per repo rules.
2. `TriggerSync.ThrownExceptions` unsubscribed + no `HasError`/`ErrorMessage` — silent failure, banned by rule 7.
3. `canSync` doesn't gate on `_syncScheduler` — button enabled but silently no-ops when scheduler absent.

Address these three before re-review. The warnings (N+1, duplicated init, double-iteration) should be fixed in the same pass.

---

### References

- [Repo ViewModel rules — domain model isolation](.claude/rules/onedrive-viewmodels.md)
- [Repo functional usage rules — silent swallowing ban](.claude/rules/functional-usage.md)
- [Repo DI rules — service lifetimes](.claude/rules/onedrive-di.md)
- [Microsoft EF Core — N+1 queries](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying#beware-of-lazy-loading)
- [ReactiveUI — ThrownExceptions](https://www.reactiveui.net/docs/handbook/commands/#handling-exceptions)
- [OWASP Top 10 — A04: Insecure Design](https://owasp.org/Top10/A04_2021-Insecure_Design/) (silent failure paths are a design weakness)
