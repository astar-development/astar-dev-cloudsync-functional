# Code Review — `AStar.Dev.CloudSyncFunctional` — 2026-05-28

Branch: `feature/27-sync-pipeline`  
Reviewed: committed tests (`6dfaf8e`) + all unstaged production code in `src/…/Sync/` + modified `Domain/`, `Graph/`  
Reviewer: Claude Code (c-sharp-reviewer)  
Supersedes: `docs/code-review-report-CloudSyncFunctional-20260528T135551Z.md` (incomplete run)

---

## References

- [Repo functional-usage rules](.claude/rules/functional-usage.md)
- [Repo onedrive-sync rules](.claude/rules/onedrive-sync.md)
- [Repo onedrive-graph rules](.claude/rules/onedrive-graph.md)
- [Repo c-sharp-code-style rules](.claude/rules/c-sharp-code-style.md)
- [Repo c-sharp-testing rules](.claude/rules/c-sharp-testing.md)
- [Microsoft — Avoid instantiating HttpClient directly](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [OWASP A04 — Insecure Design](https://owasp.org/Top10/A04_2021-Insecure_Design/)

---

## Status vs Previous Report

| Old ID | Status | Notes |
|--------|--------|-------|
| E07 (`is Ok/Fail/Some/None` pattern matching) | ✅ RESOLVED | Production code now uses `Tap`/`Match`/`MatchAsync` throughout |
| E09 (`System.IO.Path` in `DownloadJobBuilder`) | ✅ RESOLVED | Uses `fileSystem.Path` correctly |
| W05 (implicit `Option<T>` → `string` cast) | ✅ RESOLVED | Replaced with `Match` extraction |
| All others | ⚠ STILL PRESENT | See below |

---

## Issues

### ERRORS

---

#### E01 — `GraphService.cs:229` — `throw` inside `Match` lambda

**Severity:** error | **Status:** unchanged

`throw` is globally banned outside infrastructure `catch` blocks. The `Match` error branch must never throw.

```csharp
// ❌ current
private static GraphClient GetClientForToken(IGraphClientFactory factory, string accessToken)
    => factory.CreateClient(accessToken).Match(client => client, _ => throw new InvalidOperationException("Failed to create Graph client."));
```

```csharp
// ✅ fix — return Result and let callers Bind
private static Result<GraphClient, GraphError> TryGetClientForToken(IGraphClientFactory factory, string accessToken)
    => factory.CreateClient(accessToken);
```

All callers (`EnumerateFolderAsync`, `GetFolderIdByPathAsync`, `GetDownloadUrlAsync`, `UploadFileAsync`, `DeleteItemAsync`) must `Bind` on the client result before proceeding.

---

#### E02 — `GraphService.cs:99–100, 124` — `System.IO.*` in `UploadFileAsync`

**Severity:** error | **Status:** unchanged

`System.IO.*` is completely banned. `UploadFileAsync` is `static` and has no `IFileSystem`; `IFileSystem` must be injected into `GraphService`.

```csharp
// ❌ current
var fileName    = System.IO.Path.GetFileName(localPath);
var lastModified = System.IO.File.GetLastWriteTimeUtc(localPath).ToString(...);
// ...
using var fileStream = System.IO.File.OpenRead(localPath);
```

```csharp
// ✅ fix — inject IFileSystem
var fileName    = fileSystem.Path.GetFileName(localPath);
var lastModified = fileSystem.File.GetLastWriteTimeUtc(localPath)
    .ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
// ...
await using var fileStream = fileSystem.File.OpenRead(localPath);
```

---

#### E03 — `GraphService.cs:277` — Raw `new HttpClient()` in `UploadChunksAsync`

**Severity:** error | **Status:** unchanged

Raw `HttpClient` construction causes socket exhaustion. Use `IHttpClientFactory`.

```csharp
// ❌ current
using var httpClient = new System.Net.Http.HttpClient();
```

```csharp
// ✅ fix — inject IHttpClientFactory, create named client
using var httpClient = httpClientFactory.CreateClient("Graph");
```

See [Microsoft — Avoid instantiating HttpClient directly](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines).

---

#### E04 — `GraphService.cs:278–302` — `UploadChunksAsync` has no retry policy

**Severity:** error | **Status:** unchanged

`onedrive-sync.md` requires: max 5 retries per chunk, honour `Retry-After` on 429, exponential backoff on network errors, restart whole session on 404. Current code returns `null` on any non-success with no retry.

```csharp
// ❌ current — silently returns null on failure
if (!response.IsSuccessStatusCode)
    return null;
```

Fix: implement the full chunk retry loop per `onedrive-sync.md` §Upload protocol. Return `Result<string, GraphError>` instead of `string?` so callers can handle failure without null checks.

---

#### E05 — `SyncWorker.cs:27` — `job.ItemId` passed as download URL

**Severity:** error | **Status:** unchanged

`IHttpDownloader.DownloadAsync(string url, …)` expects a **pre-signed download URL**. `job.ItemId` is a Graph item identifier (`01ABC…`). Passing it to an HTTP client produces a 400/connection error — nothing will ever download.

```csharp
// ❌ current
return downloader.DownloadAsync(job.ItemId, job.LocalPath, job.RemoteModified ?? DateTimeOffset.UtcNow, null, cancellationToken);
```

`SyncWorker` must inject `IGraphService` and call `GetDownloadUrlAsync` first:

```csharp
// ✅ fix
return graphService.GetDownloadUrlAsync(accountId, accessToken, job.ItemId, cancellationToken)
    .BindAsync(url => downloader.DownloadAsync(url, job.LocalPath, job.RemoteModified ?? DateTimeOffset.UtcNow, null, cancellationToken)
        .MapAsync<Unit, SyncError, Result<Unit, SyncError>>(ok => new Ok<Unit, SyncError>(ok)));
```

---

#### E06 — `SyncWorker.cs:32` — `string.Empty` passed as `accountId` to `UploadAsync`

**Severity:** error | **Status:** unchanged

`ExecuteUploadAsync` has no `accountId` so `string.Empty` is passed to `UploadService.UploadAsync` → `GraphService.UploadFileAsync`. The drive-ID cache key is always `""`, corrupting all upload drive lookups.

```csharp
// ❌ current
uploadService.UploadAsync(string.Empty, accessToken, …)
```

Add `accountId` to `ISyncWorker.ExecuteAsync` or embed it in `UploadJob`, then thread it through.

---

#### E07 — `LocalChangeDetector.cs:51` — Remote **path** passed as Graph **folder ID** to upload job *(NEW)*

**Severity:** error

`LocalChangeDetector.WalkDirectory` computes a `parentRemotePath` (e.g. `/Documents/Work`) and passes it as the `parentFolderId` argument to `SyncJobFactory.CreateUpload`. `UploadJob.ParentFolderId` is a Graph item identifier (`01ABC…`), not a path string. The Graph SDK will reject it.

```csharp
// ❌ current — path string, not a Graph ID
var parentRemotePath = fileSystem.Path.GetDirectoryName(...)?.Replace(...) ?? remoteSyncRoot;
jobs.Add(SyncJobFactory.CreateUpload(file, remotePath, parentRemotePath));
```

Fix: `LocalChangeDetector` must have access to a path→Graph-ID mapping (from `syncedItems`) and look up the parent folder's `RemoteItemId`, or defer parent-ID resolution to `SyncWorker` via a lazy lookup through `IGraphService`.

---

#### E08 — `HttpDownloader.cs:71` — `OpenWrite` does not truncate; corrupts re-downloads *(NEW)*

**Severity:** error

`fileSystem.File.OpenWrite(localPath)` opens an existing file for writing **without truncating** it. If the new download is shorter than the existing file, the tail bytes of the old file remain, producing a corrupt file.

```csharp
// ❌ current
await using var fileStream = fileSystem.File.OpenWrite(localPath);
```

```csharp
// ✅ fix — truncate on open
await using var fileStream = fileSystem.File.Open(localPath, FileMode.Create, FileAccess.Write);
```

---

#### E09 — `SyncService.cs:94` — `ResolveConflictAsync` result silently discarded in `Skip` case *(NEW)*

**Severity:** error

When `policy == ConflictPolicy.Skip`, the call to `syncRepository.ResolveConflictAsync` is `await`-ed but the `Result<Unit, PersistenceError>` is thrown away. A persistence failure is invisible and the caller receives `Ok`.

```csharp
// ❌ current
await syncRepository.ResolveConflictAsync(new Persistence.ValueObjects.SyncConflictId(conflict.Id.Value), cancellationToken).ConfigureAwait(false);
return new Ok<Unit, SyncError>(Unit.Default);
```

```csharp
// ✅ fix
return await syncRepository.ResolveConflictAsync(new Persistence.ValueObjects.SyncConflictId(conflict.Id.Value), cancellationToken)
    .MatchAsync<Unit, Onboarding.PersistenceError, Result<Unit, SyncError>>(
        _ => new Ok<Unit, SyncError>(Unit.Default),
        error => new Fail<Unit, SyncError>(SyncErrorFactory.StorageFailed(error)));
```

---

#### E10 — `SyncedItemRegistrar.cs:31` — Result from `UpsertAsync` discarded (silent swallowing)

**Severity:** error | **Status:** unchanged

`functional-usage.md` §7: an error branch that neither logs nor surfaces is a defect. The `UpsertAsync` result is completely discarded.

```csharp
// ❌ current
await syncedItemRepository.UpsertAsync(entity, cancellationToken).ConfigureAwait(false);
syncedItems[remotePath] = entity;
```

```csharp
// ✅ fix
var upsertResult = await syncedItemRepository.UpsertAsync(entity, cancellationToken).ConfigureAwait(false);
upsertResult.Match(
    _ => syncedItems[remotePath] = entity,
    error => LogUpsertFailed(logger, remotePath, error.Message));
```

`RegisterFolderAsync` should also return `Task<Result<Unit, SyncError>>` so callers can propagate the failure. The interface must be updated accordingly.

---

#### E11 — `HttpDownloader.cs:20–21` — Intermediate unwrapping: `await` into variable then `Match` *(NEW)*

**Severity:** error

`functional-usage.md` §4 bans: "Never `await` a `Task<Result<T,E>>` into a variable just to call `Match` on the next line."

```csharp
// ❌ current
var result = await TryDownloadAsync(url, localPath, remoteModified, progress, cancellationToken).ConfigureAwait(false);
var succeeded = result.Match(_ => true, _ => false);
if (succeeded)
    return result;
```

The retry logic here can be restructured without intermediate unwrapping by returning a discriminated result type from `TryDownloadAsync`:

```csharp
// ✅ direction — let TryDownloadAsync return a richer type
private enum TryResult { Success, Throttled, Failed }

// Or restructure the loop to not inspect the Result at all —
// TryDownloadAsync either returns Ok (done), or throws/returns a typed Fail
// that the outer loop pattern-matches only in test code.
```

---

#### E12 — `RemoteFolderEnumerator.cs:33–34` and `LocalDeletionDetector.cs:54–55` — Intermediate unwrapping + wrong None-case lambda *(NEW)*

**Severity:** error

Two violations bundled:

**1 — Intermediate unwrapping (rule §4)**

```csharp
// ❌ RemoteFolderEnumerator.cs:33-34
var folderIdOption = await graphService.GetFolderIdByPathAsync(...).ConfigureAwait(false);
var folderId = folderIdOption.Match<string, string?>(id => id, _ => null);
if (folderId is null) { ... continue; }

// ❌ LocalDeletionDetector.cs:54-55
var folderId = await graphService.GetFolderIdByPathAsync(...).ConfigureAwait(false);
return await folderId.Match<string, Task<GraphError?>>(async itemId => { ... }, _ => Task.FromResult<GraphError?>(null));
```

Both `await` the `Task` into a variable and immediately call `.Match` — banned by rule §4.

**2 — `_ =>` instead of `() =>` for `Option<T>` None case**

`Option<T>.Match` None handler signature is `Func<TResult>` (parameterless). `_ =>` is a single-parameter lambda and will not compile.

```csharp
// ❌ _ => won't compile for Option<T> None handler
folderIdOption.Match<string, string?>(id => id, _ => null)

// ✅ parameterless None handler
folderIdOption.Match<string, string?>(id => id, () => null)
```

Fix both by chaining `MatchAsync` directly:

```csharp
// ✅ RemoteFolderEnumerator — chain MatchAsync, eliminate null local
await graphService.GetFolderIdByPathAsync(accessToken, account.DriveIdValue, rule.RemotePath.TrimStart('/'), cancellationToken)
    .MatchAsync(
        async folderId =>
        {
            var enumerateResult = await graphService.EnumerateFolderAsync(...);
            enumerateResult.Match(
                items => allItems.AddRange(items),
                error =>
                {
                    LogEnumerationFailed(logger, rule.RemotePath, error.Message);
                    enumerateErrors.Add(SyncErrorFactory.GraphFailed(error));
                });
        },
        () => LogFolderNotFound(logger, rule.RemotePath));
```

Note: the `continue` inside a `foreach` combined with async `MatchAsync` requires collecting errors in a list and checking after the loop rather than returning early from within the lambda.

---

#### E13 — `HttpDownloader.cs:54–59` — Double-sleep on HTTP 429

**Severity:** error | **Status:** unchanged

`TryDownloadAsync` sleeps for `Retry-After` then returns `Fail`. The outer loop in `DownloadAsync` then sleeps again with `GetBackoffDelay` before retrying — two sleeps per 429.

Fix: either (a) do NOT sleep inside `TryDownloadAsync` for 429 — return a typed `ThrottledResult` so the outer loop can sleep the correct duration once, or (b) have `TryDownloadAsync` signal the desired delay back to the caller.

---

#### E14 — `SyncPipeline.cs:46` — Per-worker `localCompleted` counter reported as global progress

**Severity:** error | **Status:** unchanged

With `workerCount = 8` each worker counts `0…n/8` independently. The UI receives unsorted, overlapping progress values.

```csharp
// ❌ current — each worker has its own counter
var localCompleted = 0;
localCompleted++;
onProgress(new SyncProgressEventArgs(accountId, remotePath, localCompleted, total, ...));
```

```csharp
// ✅ fix — shared atomic counter on the SyncPipeline instance
private int _completed;

// In RunWorkerAsync:
var completed = Interlocked.Increment(ref _completed);
onProgress(new SyncProgressEventArgs(accountId, remotePath, completed, total, ...));
```

Reset `_completed` to `0` at the start of each `RunAsync` call.

---

#### E15 — `FileClassifier.Classify` — `Contains` instead of token matching

**Severity:** error | **Status:** unchanged

`onedrive-sync.md` specifies: "tokenises the path on `/ - _ . (space)` and checks each token against rule keywords (case-insensitive)." The current implementation uses `Contains` — a substring check that causes false positives (keyword `photo` matches `/photographs/img.jpg`).

The tests in `GivenAFileClassifier` were written to match the `Contains` behaviour, not the spec. Both implementation and tests are wrong.

```csharp
// ❌ current
.Where(rule => rule.Keywords.Any(keyword => remotePath.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
```

```csharp
// ✅ fix
private static readonly char[] Separators = ['/', '-', '_', '.', ' '];

public static IReadOnlyList<FileClassification> Classify(string remotePath, IReadOnlyList<FileClassificationRule> rules)
{
    var tokens = remotePath.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
    var matches = rules
        .Where(rule => rule.Keywords.Any(kw => tokens.Any(t => t.Equals(kw, StringComparison.OrdinalIgnoreCase))))
        .Select(rule => rule.Classification)
        .ToList();

    return matches.Count > 0 ? matches : [FileClassificationFactory.CreateUnclassified()];
}
```

Update `GivenAFileClassifier` tests to match corrected behaviour.

---

### WARNINGS

---

#### W01 — `OneDriveAccount.cs:20` — Primitive `string? DriveId` alongside typed `PersistenceDriveId DriveIdValue`

**Severity:** warning | **Status:** unchanged

Two properties for the same concept. `string? DriveId` is redundant and violates primitive-obsession rules.

```csharp
// ❌ current
public string? DriveId { get; init; }
public PersistenceDriveId DriveIdValue { get; init; }
```

Remove `DriveId`. Update all callers (`SyncScheduler.TriggerAccountAsync`, `SyncScheduler.RunSyncPassAsync`) to use `DriveIdValue`.

---

#### W02 — `SyncConflict.cs:35` — `AccountId` and `RemoteItemId` as plain `string`

**Severity:** warning | **Status:** unchanged

`SyncConflict` uses raw strings for both. Typed wrappers `AccountId` and `OneDriveItemId` exist.

```csharp
// ❌ current
public sealed record SyncConflict(SyncConflictId Id, string AccountId, string RemoteItemId, …);

// ✅ fix
public sealed record SyncConflict(SyncConflictId Id, AccountId AccountId, OneDriveItemId RemoteItemId, …);
```

---

#### W03 — `SyncConflict.cs` — Missing `SyncConflictFactory`; production code constructs directly

**Severity:** warning | **Status:** unchanged (production violation now confirmed in `DownloadJobBuilder.cs:39`)

Code-style rules: every record must have a paired factory. `SyncConflict` has none. Worse, `DownloadJobBuilder.cs:39` constructs it directly with `new SyncConflict(...)` in production code — must use a factory.

```csharp
// ✅ add factory
public static class SyncConflictFactory
{
    public static SyncConflict CreatePending(SyncConflictId id, AccountId accountId, OneDriveItemId remoteItemId,
        DateTimeOffset localModifiedAt, DateTimeOffset remoteModifiedAt) =>
            new(id, accountId, remoteItemId, localModifiedAt, remoteModifiedAt, ConflictState.Pending);
}
```

---

#### W04 — `SyncRuleEvaluator.cs:16` and `RemoteFolderEnumerator.cs:22` — Domain logic takes `SyncRuleEntity` (persistence type)

**Severity:** warning | **Status:** unchanged

Both leak the persistence type `SyncRuleEntity` into the domain/sync layer. Define a `SyncRule` domain record and map at the service boundary.

```csharp
// ❌ current
public static bool IsIncluded(string remotePath, IReadOnlyList<SyncRuleEntity> rules)

// ✅ fix
public sealed record SyncRule(string RemotePath, RuleType RuleType);
public static bool IsIncluded(string remotePath, IReadOnlyList<SyncRule> rules)
```

---

#### W05 — `SyncRuleEvaluator.cs` — `GivenASyncRuleEvaluator` tests use `SyncRuleEntity` directly

**Severity:** warning | **Status:** dependent on W04

Once W04 is fixed (domain type introduced), these tests must switch to constructing `SyncRule` records. The test helper `BuildRule` will need to be updated accordingly.

---

#### W06 — `LocalDeletionDetector.cs:54` — `remotePath` not trimmed before `GetFolderIdByPathAsync`

**Severity:** warning | **Status:** unchanged

`RemoteFolderEnumerator` trims `rule.RemotePath.TrimStart('/')` before calling `GetFolderIdByPathAsync`. `LocalDeletionDetector` passes `remotePath` (from `item.RemotePath`) directly — may contain a leading `/`, producing a double-slash path and 404s for items that exist.

```csharp
// ❌ current
var folderId = await graphService.GetFolderIdByPathAsync(accessToken, account.DriveIdValue, remotePath, cancellationToken);

// ✅ fix
var folderId = await graphService.GetFolderIdByPathAsync(accessToken, account.DriveIdValue, remotePath.TrimStart('/'), cancellationToken);
```

---

#### W07 — `DownloadJobBuilder.cs:19` and `LocalChangeDetector.cs:17` — Mutable `List<SyncJob>` return type

**Severity:** warning | **Status:** unchanged (now confirmed in both classes)

Both `Build` and `Detect` return `List<SyncJob>`, which callers can mutate. Return `IReadOnlyList<SyncJob>`.

```csharp
// ✅
public IReadOnlyList<SyncJob> Build(…)
public IReadOnlyList<SyncJob> Detect(…)
```

---

#### W08 — Pipeline step classes have no interfaces; injected as concrete types *(NEW)*

**Severity:** warning

`RemoteFolderEnumerator`, `RemoteDeletionDetector`, `LocalDeletionDetector`, `DownloadJobBuilder`, `LocalChangeDetector`, and `JobExecutor` have no interfaces. `SyncService` injects them as concrete types directly. This:

- Breaks the DI contract ("inject interfaces, not implementations")
- Makes unit testing `SyncService` in isolation impossible without a real `IGraphService` etc.
- Prevents alternative implementations

Each pipeline step must define a matching interface (`IRemoteFolderEnumerator`, `IRemoteDeletionDetector`, etc.) and `SyncService` must inject those interfaces.

---

#### W09 — `SyncScheduler.cs` — Duplicate entity-to-domain mapping in two methods *(NEW)*

**Severity:** warning

`TriggerAccountAsync(string)` (lines 64–71) and `RunSyncPassAsync` (lines 147–155) contain identical `OneDriveAccount` construction blocks. This is a maintainability hazard and DRY violation.

```csharp
// Extract to private helper
private static OneDriveAccount MapToDomain(AccountEntity entity) =>
    new()
    {
        AccountId    = Auth.AccountId.Create(entity.Id.Value),
        IsActive     = entity.IsActive,
        DriveIdValue = new Persistence.ValueObjects.DriveId(entity.DriveId.Value),
        SyncConfig   = entity.SyncConfig
    };
```

---

#### W10 — `SyncJob.cs` — Record properties use primitive `string` for domain concepts *(NEW)*

**Severity:** warning

`DownloadJob.ItemId`, `RemotePath`, `LocalPath`, `ETag` and `UploadJob.LocalPath`, `RemotePath`, `ParentFolderId` are all plain strings. Per persistence rules, `RemotePath` and `LocalPath` have typed wrappers. At minimum `RemotePath` → `RemotePath` and `LocalPath` → `LocalPath` (typed). `ParentFolderId` is effectively an `OneDriveItemId`.

---

#### W11 — `SyncService.SyncAccountAsync` — MatchAsync error branches missing logging *(NEW)*

**Severity:** warning

`functional-usage.md` §7: every `Match`/`MatchAsync` error branch must log. Several error branches in `SyncService` silently assign to a local variable without logging:

```csharp
// ❌ no logging in error branch
await remoteFolderEnumerator.EnumerateAsync(...)
    .MatchAsync(items => remoteItems = items, error => enumerateError = error);
```

Each error branch must call the appropriate `[LoggerMessage]` partial method before setting the local.

---

#### W12 — `SyncService` constructor has 11 parameters

**Severity:** warning

`SyncService` primary constructor takes 11 dependencies. Code-style rules recommend ≤5. Consider introducing a `SyncPipelineSteps` parameter object grouping the six pipeline-step dependencies, or decompose `SyncService` into smaller coordinators.

---

#### W13 — Missing unit tests for new production implementations

**Severity:** warning | **Status:** unchanged (all implementations now visible)

| File | What to test |
|------|-------------|
| `HttpDownloader.cs` | Retry behaviour (429, network errors, double-sleep bug), file truncation on re-download, timestamp set after write |
| `UploadService.cs` | `GraphError` → `SyncError` mapping |
| `SyncWorker.cs` | Download URL resolution (E05 fix), upload account-ID threading (E06 fix) |
| `SyncPipeline.cs` | Worker count, channel backpressure, global vs per-worker progress (E14 fix) |
| `DownloadJobBuilder.cs` | eTag match/mismatch, timestamp tolerance, conflict detection, local-path construction |
| `LocalChangeDetector.cs` | Hidden-file skip, extension skip, upload job generation, parent-ID resolution (E07 fix) |
| `RemoteDeletionDetector.cs` | File deleted locally, tracking record removed |
| `LocalDeletionDetector.cs` | Path trimming, Graph delete, tracking removal |
| `RemoteFolderEnumerator.cs` | Root-rule deduplication, ancestor filtering |
| `SyncedItemRegistrar.cs` | Folder creation, UpsertAsync failure surface |
| `SyncScheduler.cs` | Timer interval, trigger, per-account cancel, disposal |
| `SyncService.cs` | Full pipeline orchestration, conflict detection, token failure abort |

---

### SUGGESTIONS

---

#### S01 — `DeltaItem.cs` — No `DeltaItemFactory`

**Severity:** suggestion | **Status:** unchanged

Code-style rules: "Accompany each record `<name>` with a corresponding `<name>Factory`." `FileDeltaItem`, `FolderDeltaItem`, `DeletedDeltaItem` have no `DeltaItemFactory`.

---

#### S02 — `SyncProgressEventArgs` and `JobCompletedEventArgs` — `AccountId` and path fields as plain strings

**Severity:** suggestion | **Status:** unchanged

`AccountId`, `CurrentFile`, `RemotePath` in both event-arg records are plain strings. Typed wrappers `AccountId` and `RemotePath` exist. Lower priority (event args, not domain types) but applying them consistently prevents primitive-obsession creep.

---

#### S03 — `JobExecutor` is a zero-value thin wrapper *(NEW)*

**Severity:** suggestion

`JobExecutor.ExecuteAsync` is a single-statement forwarding call that rearranges parameter order:

```csharp
public Task ExecuteAsync(..., Action<SyncProgressEventArgs> onProgress, Action<JobCompletedEventArgs> onJobCompleted, ...)
    => syncPipeline.RunAsync(jobs, accessToken, onProgress, onJobCompleted, accountId, workerCount, cancellationToken);
```

Code-style rules: "A private method whose entire body is a 1–2 combinator chain that forwards to another method earns nothing." The same logic applies to a class. Inline `syncPipeline.RunAsync(...)` directly in `SyncService` and remove `JobExecutor`.

---

#### S04 — `SyncService.SyncAccountAsync` — Mutable-locals + `MatchAsync` side-effect pattern *(NEW)*

**Severity:** suggestion

The sequential pipeline is written using nullable mutable locals (`string? accessToken`, `SyncError? enumerateError`, etc.) that are assigned inside `MatchAsync` side-effect lambdas and then checked with `if (x is not null)`. This is equivalent to `if (result is Ok/Fail)` but harder to read.

Consider refactoring the pipeline into a `BindAsync` chain where each step maps the accumulated state forward:

```csharp
// ✅ direction — thread state through Bind rather than mutable locals
return await authService.AcquireTokenSilentAsync(account.AccountId.Value, cancellationToken)
    .MapAsync(auth => auth.AccessToken)
    .BindAsync(accessToken => RunSyncStepsAsync(account, accessToken, rules, cancellationToken))
    .MatchAsync(
        _ => (Result<Unit, SyncError>)new Ok<Unit, SyncError>(Unit.Default),
        error => new Fail<Unit, SyncError>(error));
```

This also resolves W11 since error handling becomes explicit per-step.

---

## Summary

| Severity | Count | New | Resolved vs prev report |
|----------|-------|-----|------------------------|
| error | 15 | 5 (E07, E08, E09, E11, E12) | 3 (old E07 pattern-match, old E09 IO.Path, old W05) |
| warning | 13 | 6 (W08–W13) | 0 |
| suggestion | 4 | 2 (S03, S04) | 0 |

### Verdict: **Request Changes**

The new implementation has made genuine progress: production code now uses `Tap`/`Match`/`MatchAsync` throughout (old E07 resolved), `DownloadJobBuilder` correctly uses `IFileSystem` (old E09 resolved). These were the two highest-friction issues to retrofit.

However four categories of defect still block this PR:

1. **Infrastructure boundary violations (E02, E03) and banned `throw` (E01)** — `System.IO.*` and raw `new HttpClient()` remain in `GraphService`, and `GetClientForToken` still throws inside a `Match` lambda.

2. **Correctness bugs in the download/upload path (E05, E06, E07, E08)** — `SyncWorker` passes a Graph item ID as a download URL, passes `string.Empty` as the account ID to upload, `LocalChangeDetector` passes a path string where a Graph folder ID is required, and `HttpDownloader.OpenWrite` will corrupt re-downloaded files.

3. **Functional paradigm violations (E11, E12)** — Three methods await a `Task<Result>` or `Task<Option>` into an intermediate variable and then call `.Match` on the next line (banned by rule §4). The `Option<T>` None-case lambdas use `_ =>` instead of `() =>` and will not compile.

4. **Missing retry policy and atomic progress counter (E04, E13, E14)** — Upload chunk retries are absent, the download 429 double-sleep remains, and per-worker progress counters produce disjoint UI updates.

All error-severity issues must be resolved before merging.
