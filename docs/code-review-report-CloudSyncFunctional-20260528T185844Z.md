# Code Review — `AStar.Dev.CloudSyncFunctional` — 2026-05-28 (pass 3)

Branch: `feature/27-sync-pipeline`  
Reviewed: all unstaged production + test changes; committed tests; new interface files; `FunctionalParadigm` modifications  
Reviewer: Claude Code (c-sharp-reviewer)  
Supersedes: `docs/code-review-report-CloudSyncFunctional-20260528T152449Z.md`

---

## References

- [Repo functional-usage rules](.claude/rules/functional-usage.md)
- [Repo onedrive-sync rules](.claude/rules/onedrive-sync.md)
- [Repo onedrive-graph rules](.claude/rules/onedrive-graph.md)
- [Repo c-sharp-code-style rules](.claude/rules/c-sharp-code-style.md)
- [Repo c-sharp-testing rules](.claude/rules/c-sharp-testing.md)
- [Microsoft — IHttpClientFactory](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)

---

## Progress vs Previous Report

Substantial fixes applied. 23 of 32 prior findings are resolved.

| Category | Was | Now | Net |
|----------|-----|-----|-----|
| Error | 15 | 3 | −12 |
| Warning | 13 | 7 | −6 |
| Suggestion | 4 | 5 | +1 |

### Resolved (all 15 prior errors + 8 warnings)

| Old ID | Issue | Status |
|--------|-------|--------|
| E01 | `throw` in `Match` lambda | ✅ `TryGetClientForToken` returns `Result`, callers `Bind` |
| E02 | `System.IO.*` in `UploadFileAsync` | ✅ uses `fileSystem.*` |
| E03 | Raw `new HttpClient()` | ✅ uses `httpClientFactory.CreateClient("Graph")` |
| E04 | No retry in `UploadChunksAsync` | ✅ full retry loop with 429/404/backoff |
| E05 | `job.ItemId` passed as download URL | ✅ `GetDownloadUrlAsync` called first |
| E06 | `string.Empty` as `accountId` in upload | ✅ `accountId`/`driveId` threaded through `ISyncWorker.ExecuteAsync` |
| E07 | Remote path passed as Graph folder ID | ✅ `UploadJob` now carries `ParentFolderPath`; `SyncWorker` resolves to ID |
| E08 | `OpenWrite` corrupts re-downloads | ✅ `File.Open(FileMode.Create)` |
| E09 | `ResolveConflictAsync` Skip case silently discarded | ✅ unified `MatchAsync` handler |
| E10 | `UpsertAsync` result discarded in registrar | ✅ `Match` handles both branches, logs on failure |
| E11 | Intermediate unwrapping in `HttpDownloader` | ✅ retry loop restructured, no intermediate `result` variable |
| E12 | Intermediate unwrapping + wrong `_ =>` for None | ✅ `ProcessRuleAsync` chains `MatchAsync` directly; `_ =>` is correct (`None<T>` converts to `string`) |
| E13 | Double-sleep on HTTP 429 | ✅ single `await Task.Delay` then `continue` |
| E14 | Per-worker progress counter | ✅ `Interlocked.Increment(ref _completed)`, reset each `RunAsync` |
| W01 | `string? DriveId` beside `DriveIdValue` | ✅ `DriveId` removed |
| W02 | `SyncConflict` primitive strings | ✅ typed `PersistenceAccountId`, `OneDriveItemId` |
| W03 | Missing `SyncConflictFactory` | ✅ added, `DownloadJobBuilder` uses it |
| W04 | `SyncRuleEvaluator` took `SyncRuleEntity` | ✅ `SyncRule` domain record + `SyncRuleFactory` added |
| W05 | Tests used `SyncRuleEntity` directly | ✅ tests use `SyncRuleFactory` |
| W06 | Missing `TrimStart` in `LocalDeletionDetector` | ✅ `remotePath.TrimStart('/')` added |
| W07 | Mutable `List<SyncJob>` return types | ✅ both return `IReadOnlyList<SyncJob>` |
| W08 | No interfaces for pipeline steps | ✅ all six pipeline steps have interfaces; `SyncService` injects by interface |
| W09 | Duplicate entity-to-domain mapping | ✅ `MapToDomain` static helper extracted |

**Incorrect prior finding — retracted:**  
> Old E12b: `_ =>` for `Option<T>` None handler "won't compile"  
`OptionExtensions.Match` None handler is `Func<string, TOut>` — `None<T>` converts to `string` implicitly, so `_ =>` is valid. Error was wrong.

---

## Issues

### ERRORS

---

#### E01 — `FileClassifier.cs:20` — Token matching still uses `Contains`, not `Equals`

**Severity:** error

The previous fix tokenised the path correctly. However the inner comparison was not corrected — it still uses `t.Contains(kw, …)`. A token `"photographs"` still matches keyword `"photo"`, which violates the spec.

```csharp
// ❌ current — still substring, not exact token match
.Where(rule => rule.Keywords.Any(kw => tokens.Any(t => t.Contains(kw, StringComparison.OrdinalIgnoreCase))))
```

```csharp
// ✅ fix — exact token comparison
.Where(rule => rule.Keywords.Any(kw => tokens.Any(t => t.Equals(kw, StringComparison.OrdinalIgnoreCase))))
```

**Tests must also be updated.** `GivenAFileClassifier.when_path_contains_keyword_then_classified` uses keyword `"photo"` against path `/Photos/holiday.jpg`. Token `"Photos"` ≠ `"photo"` with `Equals` — the test will fail. The test rule must use keyword `"Photos"` (exact token) and the suite should add a **false-positive test**:

```csharp
[Fact]
public void when_keyword_is_substring_of_token_then_not_classified()
{
    var classification = new FileClassification("Photos", ["photo"]);
    var rule = new FileClassificationRule(classification, ["photo"]);
    var rules = new[] { rule };

    // "photographs" token must NOT match keyword "photo" (substring, not exact)
    var result = FileClassifier.Classify("/photographs/img.jpg", rules);

    result.ShouldHaveSingleItem();
    result[0].Name.ShouldBe("Unclassified");
}
```

---

#### E02 — `SyncScheduler.cs:76` — `SyncAccountAsync` result silently discarded; `SyncCompleted` fires on failure *(NEW)*

**Severity:** error

`TriggerAccountAsync(OneDriveAccount)` awaits `SyncAccountAsync` but discards the `Result<Unit, SyncError>`. `SyncCompleted` fires unconditionally — callers receive a success signal when sync has failed. The failure is invisible.

```csharp
// ❌ current — result thrown away, SyncCompleted fires regardless
await syncService.SyncAccountAsync(account, cts.Token).ConfigureAwait(false);
SyncCompleted?.Invoke(this, account.AccountId.Value);
```

```csharp
// ✅ fix
await syncService.SyncAccountAsync(account, cts.Token)
    .MatchAsync(
        _ => SyncCompleted?.Invoke(this, account.AccountId.Value),
        error => LogSyncFailed(logger, account.AccountId.Value, error.Message));
```

Add `LogSyncFailed` as a `[LoggerMessage]` partial method.

---

#### E03 — `App.axaml.cs` — `IHttpClientFactory` not registered; `GraphService` and `HttpDownloader` will fail at runtime *(NEW)*

**Severity:** error

`GraphService` now takes `IHttpClientFactory httpClientFactory` and `HttpDownloader` also depends on it. `App.axaml.cs` has no `services.AddHttpClient()` call. DI resolution will throw `InvalidOperationException` at startup when either service is requested.

```csharp
// ❌ current — IHttpClientFactory absent from container
services.AddSingleton<IGraphService, GraphService>();
services.AddSingleton<IHttpDownloader, HttpDownloader>();
// ...no AddHttpClient() call anywhere

// ✅ fix — add before the graph/download registrations
services.AddHttpClient();
// or add a named client:
services.AddHttpClient("Graph");
```

---

### WARNINGS

---

#### W01 — `SyncScheduler.cs:76` — `SyncAccountAsync` result not handled (also see E02)

See E02 above. Beyond correctness, the pattern also violates `functional-usage.md` §7: "an error branch that neither logs nor surfaces is a defect." The `MatchAsync` fix in E02 also satisfies this rule.

---

#### W02 — `SyncService.cs:67, 73` — `MatchAsync` error branches do not log

**Severity:** warning

`functional-usage.md` §7 requires every `Match`/`MatchAsync` error branch to log.

```csharp
// ❌ no log in error branch
var remoteDeletionError = await remoteDeletionDetector.DetectAndDeleteAsync(...)
    .MatchAsync<Unit, SyncError, SyncError?>(_ => null, error => error);

var localDeletionError = await localDeletionDetector.DetectAsync(...)
    .MatchAsync<Unit, SyncError, SyncError?>(_ => null, error => error);
```

```csharp
// ✅ fix — log then return
var remoteDeletionError = await remoteDeletionDetector.DetectAndDeleteAsync(...)
    .MatchAsync<Unit, SyncError, SyncError?>(
        _ => null,
        error => { LogSyncFailed(logger, account.AccountId.Value, error.Message); return error; });
```

---

#### W03 — `SyncJob.cs` — Record properties use primitive `string` for domain concepts

**Severity:** warning | **Status:** unchanged

`DownloadJob.ItemId`, `RemotePath`, `LocalPath` and `UploadJob.LocalPath`, `RemotePath`, `ParentFolderPath` are all plain strings. Typed wrappers `RemotePath`, `LocalPath`, and `OneDriveItemId` exist. Apply them consistently.

---

#### W04 — `DownloadJobBuilder.cs:19` and `IDownloadJobBuilder.cs:15` — `accountId` parameter still plain `string`

**Severity:** warning

`Build(… string accountId …)` should accept a typed `AccountId`. The typed wrapper is used within the method body (`new AccountId(accountId)`) but the public API still takes `string`.

```csharp
// ❌ current
IReadOnlyList<SyncJob> Build(…, string accountId, …)

// ✅ fix
IReadOnlyList<SyncJob> Build(…, AccountId accountId, …)
```

Update `SyncService.cs:79` to pass `new AccountId(account.AccountId.Value)` (or use the already-created `accountId` value object).

---

#### W05 — `SyncRule.cs:8` — `RemotePath` field is plain `string`

**Severity:** warning

`SyncRule.RemotePath` should use the typed `RemotePath` wrapper per the domain rules. Both `SyncRuleFactory` and callers must be updated.

```csharp
// ❌ current
public sealed record SyncRule(string RemotePath, RuleType RuleType);

// ✅ fix
public sealed record SyncRule(RemotePath RemotePath, RuleType RuleType);
```

---

#### W06 — `OptionExtensions.cs:46–52` — Single-handler `Match` overload has implicit-conversion trap

**Severity:** warning

The `Match<TResult, TOut>(this Option<TResult>, Func<TResult, TOut> onSome)` overload returns `new None<TOut>()` for the None case, where the declared return type is `TOut`. This compiles only due to implicit conversions and silently returns `default!` when `TOut` is not itself an `Option<T>`. The overload is undocumented and dangerous.

```csharp
// ❌ None case returns Option<TOut>, not TOut — only safe when TOut : Option<T>
public static TOut Match<TResult, TOut>(this Option<TResult> option, Func<TResult, TOut> onSome)
    => option switch
        {
            Some<TResult> some => onSome(some.Value),
            None<TResult>      => new None<TOut>(),   // ← implicit conversion, silent default!
            _ => throw new InvalidOperationException(...)
        };
```

Either add an XML doc comment explicitly stating the constraint (`TOut` must be `Option<T>`), or remove the overload and require callers to use the two-handler `Match`.

---

#### W07 — Missing unit tests

**Severity:** warning | **Status:** partially addressed (`GivenAGraphService` added)

`GivenAGraphService` gives good coverage of `GetRootFoldersAsync`. Remaining gaps:

| File | What to test |
|------|-------------|
| `HttpDownloader.cs` | Retry on 429 (honour `Retry-After`), network retry, file truncation on re-download, timestamp preserved |
| `UploadService.cs` | `GraphError` → `SyncError` mapping |
| `SyncWorker.cs` | Download URL resolution chain, upload parent-path → Graph ID lookup |
| `SyncPipeline.cs` | Global atomic counter, channel backpressure |
| `DownloadJobBuilder.cs` | eTag match/mismatch, timestamp tolerance, conflict detection, local-path construction |
| `LocalChangeDetector.cs` | Hidden-file skip, extension skip, upload job generation |
| `RemoteDeletionDetector.cs` | File deleted locally, tracking record removed |
| `LocalDeletionDetector.cs` | Graph delete on local deletion, tracking removal |
| `RemoteFolderEnumerator.cs` | Root-rule deduplication, ancestor filtering, folder-not-found skips |
| `SyncedItemRegistrar.cs` | Directory creation, upsert failure logged |
| `SyncScheduler.cs` | Sync failure propagation (E02 fix), per-account cancel, disposal |
| `SyncService.cs` | Token failure aborts, conflict detection callback |
| `FileClassifier.cs` | False-positive case (see E01) |

---

### SUGGESTIONS

---

#### S01 — `DeltaItem.cs` — No `DeltaItemFactory`

**Severity:** suggestion | **Status:** unchanged

`FileDeltaItem`, `FolderDeltaItem`, `DeletedDeltaItem` have no `DeltaItemFactory`. Code-style rules: "Accompany each record with a corresponding factory."

---

#### S02 — `SyncProgressEventArgs` / `JobCompletedEventArgs` — `AccountId` and path fields as plain strings

**Severity:** suggestion | **Status:** unchanged

`AccountId`, `CurrentFile`, `RemotePath` in both event-arg records are plain strings. Typed wrappers exist. Lower priority but prevents primitive-obsession creep.

---

#### S03 — `JobExecutor` is a zero-value forwarding wrapper

**Severity:** suggestion | **Status:** unchanged

`JobExecutor.ExecuteAsync` is a one-line parameter-rearranging call to `ISyncPipeline.RunAsync`. Code-style rules say zero-value wrappers should be inlined. Injecting `ISyncPipeline` directly into `SyncService` and calling `.RunAsync` would eliminate a class, an interface, and a DI registration with no loss.

---

#### S04 — `SyncService.SyncAccountAsync` — Mutable-locals + `MatchAsync` side-effect pattern

**Severity:** suggestion | **Status:** unchanged

The pipeline uses nullable mutable locals (`accessToken`, `enumerateError`, etc.) set via `MatchAsync` side effects, then checked with `if (x is not null)`. The cleaner approach threads state via `BindAsync`:

```csharp
// ✅ direction
return await authService.AcquireTokenSilentAsync(account.AccountId.Value, cancellationToken)
    .MapAsync(auth => auth.AccessToken)
    .BindAsync(token => RunSyncCoreAsync(account, token, cancellationToken))
    .MatchAsync<Unit, SyncError, Result<Unit, SyncError>>(
        _ => new Ok<Unit, SyncError>(Unit.Default),
        error => new Fail<Unit, SyncError>(error));
```

Eliminates S04 and resolves W02 simultaneously since every error branch in `RunSyncCoreAsync` would have a natural home.

---

#### S05 — `GivenAGraphService.cs:235` — Leftover `// ... existing code ...` comment *(NEW)*

**Severity:** suggestion

A placeholder comment was left between test methods. Remove it.

```csharp
// ❌ line 235
// ... existing code ...
```

---

## Summary

| Severity | Count | New | Resolved vs prev |
|----------|-------|-----|-----------------|
| error | 3 | 2 (E02, E03) | 12 |
| warning | 7 | 1 (effective: W01 is same root as E02) | 6 |
| suggestion | 5 | 1 (S05) | 0 |

### Verdict: **Approve with mandatory fixes**

This pass represents a major improvement. All infrastructure violations, functional-paradigm bans, and the two correctness bugs in the download/upload path are gone. The codebase is now substantially cleaner.

**Three blocking issues remain:**

1. **E01** — `FileClassifier` still uses `Contains` not `Equals` for token comparison. The spec is still violated and the tests validate the wrong behaviour.
2. **E02** — `SyncScheduler` discards the sync result and fires `SyncCompleted` on failure. Callers get a false success signal.
3. **E03** — `IHttpClientFactory` is not registered in DI. The app will not start.

These three can all be fixed in a single small commit. Warnings W03–W05 (primitive strings in `SyncJob`, `DownloadJobBuilder`, `SyncRule`) are the remaining significant technical-debt items and should be tracked as follow-up issues if not addressed in this PR.
