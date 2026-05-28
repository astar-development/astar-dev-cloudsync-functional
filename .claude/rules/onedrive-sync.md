# OneDrive Sync Pipeline

The sync pipeline runs per-account and follows six ordered steps. Each step is a discrete class. No step is skipped; they always run in this sequence.

## Pipeline steps (in order)

```
1. RemoteFolderEnumerator   — fetch all DeltaItems from selected remote folders
2. RemoteDeletionDetector   — delete local files whose remote counterpart has gone
3. LocalDeletionDetector    — delete remote files the user deleted locally
4. DownloadJobBuilder       — build download jobs for new/changed remote files
5. LocalChangeDetector      — build upload jobs for new/changed local files
6. JobExecutor              — execute all download and upload jobs (parallel)
```

After all jobs execute, `SyncedAt` is updated on the `AccountEntity` and the in-memory `OneDriveAccount`.

## Sync rules (include/exclude)

`SyncRuleEntity` has two types:

- `RuleType.Include` — the remote path is synced
- `RuleType.Exclude` — the remote path is excluded

### Evaluation (`SyncRuleEvaluator.IsIncluded`)

- Match by path prefix (`string.StartsWith`, ordinal-ignore-case).
- The character immediately after the prefix must be `/` or end-of-string — prevents `/Documents` matching `/DocumentsBackup`.
- **Most-specific wins** (longest matching prefix).
- Tie on length → `Exclude` wins.
- **No match → Exclude** (default-deny).

```csharp
// correct
bool included = SyncRuleEvaluator.IsIncluded(remotePath, rules);
```

Never implement custom rule logic outside `SyncRuleEvaluator`.

## Remote folder enumeration — deduplication

`RemoteFolderEnumerator` only enumerates **root** include rules. A rule is a root rule if no other include rule is a path ancestor of it. This prevents double-enumeration when both a parent and a child folder are selected.

Example: if `/Documents` and `/Documents/Work` are both include rules, only `/Documents` is enumerated — it covers `/Documents/Work` in its traversal.

Algorithm:
- From all `RuleType.Include` rules, keep only those where no other include rule is a prefix ancestor.
- Enumerate only the kept rules.

## File classification

`FileClassifier.Classify(remotePath, rules)` tokenises the path on `/ - _ . (space)` and checks each token against rule keywords (case-insensitive).

- Returns all matching `FileClassification` values.
- Returns `[FileClassificationFactory.CreateUnclassified()]` when no rules match.

`FileClassificationRule` has a list of `Keywords` and an associated `FileClassification`. Multiple rules can match the same file (additive classification).

File classification is **evaluated during sync enumeration** and stored in `SyncedItemClassificationEntity`. It does not affect whether a file is synced (that is controlled by sync rules).

## Local file filtering

Before treating a local file as a candidate for upload:

- Skip files with `FileAttributes.Hidden` or name starting with `.`.
- Skip `.tmp`, `.temp`, `.partial` extensions.
- Skip hidden or dot-prefixed directories during directory scan.
- Modification comparison uses a **5-second tolerance**: `localModified <= known.RemoteModifiedAt.AddSeconds(5)` means no upload needed.

## Download decision — eTag then timestamp

`DownloadJobBuilder` applies checks in this order for each `FileDeltaItem`:

1. **eTag match** — if stored eTag equals remote eTag AND local file exists:
   - Timestamps also match (within 5s tolerance) → **skip** (already in sync)
   - Timestamps differ → **conflict** (eTag match but local file has been touched)
2. **No eTag or eTag mismatch** — fall through to timestamp comparison:
   - Local file exists, tracking record exists, local modified > stored remote modified → **conflict**
   - Local file exists, **no tracking record** → **conflict** (unknown local file)
   - Otherwise → **download**

### On download

After writing the file:
- Set local file `CreationTime` and `LastWriteTime` to the remote `LastModified` timestamp.
- Use `IFileSystem` — never `System.IO` directly.

## ISyncedItemRegistrar

A discrete `ISyncedItemRegistrar` handles two registration tasks that occur during the build phase (not job execution):

```csharp
Task RegisterFolderAsync(AccountId accountId, FolderDeltaItem item, string remotePath, string localPath, Dictionary<string, SyncedItemEntity> syncedItems, CancellationToken cancellationToken);
```

- Creates the local directory via `IFileSystem` if it does not exist.
- Upserts a `SyncedItemEntity` (folder) so future syncs track it.

Phantom file registration (local file exists with no tracking record) is **not** performed — this case is treated as a conflict (see above).

## Upload protocol (resumable sessions)

### Why resumable

All uploads use the Graph API resumable upload session — even small files. Uniform code path, supports interruption and retry.

### Chunk size

**10 MB** (`10 * 1024 * 1024` bytes). Must be a multiple of 320 KB per the Graph API requirement. Do not change without verifying divisibility.

### Flow

```
1. POST /drives/{driveId}/items/{parentId}:/{filename}:/createUploadSession
   Body: { "@microsoft.graph.conflictBehavior": "replace",
           "name": filename,
           "fileSystemInfo": { "lastModifiedDateTime": "..." } }
   → response: { "uploadUrl": "..." }

2. For each chunk:
   PUT <uploadUrl>
   Headers:
     Content-Range: bytes {start}-{end}/{total}
     Content-Length: {chunkBytes}

   202 Accepted  → continue to next chunk
   200 OK / 201  → upload complete; parse { "id": "..." } from body
   404           → session expired; restart from step 1
   429           → throttled; read Retry-After header, sleep, retry same chunk
   Other 4xx/5xx → treat as error; apply exponential backoff
```

### Retry policy

- **Maximum retries**: 5 per chunk.
- **429 (Too Many Requests)**: read `Retry-After` header (delta or absolute date); sleep; retry same chunk. If no header, use exponential backoff.
- **Network errors (`HttpRequestException`)**: exponential backoff; retry same chunk.
- **Exponential backoff formula**: `min(2^(attempt-1) * 2s, 120s) + jitter(20%)`.
- **Session 404**: restart the entire session from step 1. Do not count as a chunk retry.

```csharp
private static TimeSpan GetBackoffDelay(int attempt)
{
    double seconds = Math.Min(2.0 * Math.Pow(2, attempt - 1), 120.0);
    double jitter = seconds * 0.2 * Random.Shared.NextDouble();
    return TimeSpan.FromSeconds(seconds + jitter);
}
```

### Upload session request body

Set `fileSystemInfo.lastModifiedDateTime` from the local file's `LastWriteTimeUtc` in `"yyyy-MM-ddTHH:mm:ss.fffZ"` (invariant culture) so OneDrive preserves the original modification timestamp.

## Download protocol (IHttpDownloader)

`IHttpDownloader` is a discrete interface. It handles pre-signed URL downloads with the same retry/backoff policy as uploads:

- Uses `IHttpClientFactory` — never a raw `HttpClient` field.
- Sets `User-Agent` header on every request.
- Downloads with `HttpCompletionOption.ResponseHeadersRead` to stream large files.
- On success: writes file to `localPath` via `IFileSystem`, then sets `CreationTime` and `LastWriteTime` to `remoteModified`.
- **Maximum retries**: 5. **429**: honour `Retry-After`. **Network errors**: exponential backoff.
- Returns `Result<Unit, SyncError>`.

```csharp
Task<Result<Unit, SyncError>> DownloadAsync(string url, string localPath, DateTimeOffset remoteModified, IProgress<long>? progress = null, CancellationToken cancellationToken = default);
```

## Conflict detection

A conflict occurs in these cases:

1. eTag match + local timestamp differs from remote timestamp (5s tolerance)
2. Local file modified since last sync AND remote file also changed (both differ from baseline)
3. Local file exists with no tracking record (unknown local file)

### ConflictPolicy

```csharp
public enum ConflictPolicy
{
    KeepLocal,    // discard remote, re-upload local version
    KeepRemote,   // discard local, download remote version
    Skip          // leave both versions untouched, mark as unresolved in DB
}
```

### Resolution flow

1. `ConflictResolver.Resolve(policy, localModified, remoteModified)` returns `ConflictOutcome`.
2. `ConflictApplier.ApplyAsync(conflict, outcome, accountId, accessToken, cancellationToken)` executes the chosen action via `IGraphService` or local file system.
3. `syncRepository.ResolveConflictAsync(conflictId, policy)` marks the conflict resolved in the DB.
4. Unresolved conflicts surface in the UI via `ISyncService.ConflictDetected` event.

### SyncConflict data model

```csharp
record SyncConflict(
    SyncConflictId Id,
    RemoteItemRef Remote,          // accountId + remote item id
    ConflictSnapshot Snapshot,     // LocalModified, RemoteModified timestamps
    ConflictState State            // Pending | Resolved
);
```

## Parallel job execution (ISyncPipeline)

`JobExecutor` delegates to `ISyncPipeline`, which runs jobs concurrently using a bounded `Channel<SyncJob>`:

- **Worker count**: taken from `account.SyncConfig.WorkerCount` (1–10, default 8).
- **Channel capacity**: `workerCount × 4` — applies backpressure so memory stays flat regardless of job count.
- **Producer**: writes jobs into the channel one at a time; blocks when channel is full.
- **Consumers**: `workerCount` workers drain the channel concurrently via `ISyncWorker`.
- `ISyncWorkerFactory` creates one `ISyncWorker` per worker slot.
- After all workers finish, completed job records are cleared from the DB.

```csharp
// ISyncPipeline
Task RunAsync(IEnumerable<SyncJob> jobs, string accessToken, Action<SyncProgressEventArgs> onProgress, Action<JobCompletedEventArgs> onJobCompleted, string accountId, int workerCount, CancellationToken cancellationToken = default);
```

Never hard-code `workerCount` — always pass `account.SyncConfig.WorkerCount`.

## ISyncService contract

```csharp
event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;
event EventHandler<JobCompletedEventArgs>?  JobCompleted;
event EventHandler<SyncConflict>?           ConflictDetected;

Task<Result<Unit, SyncError>> SyncAccountAsync(OneDriveAccount account, CancellationToken cancellationToken = default);
Task<Result<Unit, SyncError>> ResolveConflictAsync(SyncConflict conflict, ConflictPolicy policy, CancellationToken cancellationToken = default);
```

## SyncProgressEventArgs

```csharp
record SyncProgressEventArgs(string AccountId, string CurrentFile, int Completed, int Total, string StatusMessage, SyncState State);
```

`SyncState` enum: `Idle | Syncing | Error`.

## SyncJob types

`SyncJob` is a discriminated union:

- `DownloadJob` — remote → local
- `UploadJob` — local → remote

Constructed via `SyncJobFactory.CreateDownload(...)` and `SyncJobFactory.CreateUpload(...)`. Never construct directly.

## Token acquisition in sync

`SyncService.SyncAccountAsync` always calls `AcquireTokenSilentAsync` first. On `AuthFailedError` the sync is aborted and the result is `Result.Fail(SyncErrorFactory.AuthFailed(error))`. It never triggers interactive sign-in — that only happens from the UI.
