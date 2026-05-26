# Microsoft Graph API — OneDrive Operations

This repo uses `Microsoft.Graph` (SDK v5+) via a `GraphServiceClient` created per-access-token. All public `IGraphService` methods return `Result<T, GraphError>`. Never use `string` as the error type — see `@.claude/rules/functional-usage.md` for the `GraphError` discriminated union definition.

## Packages required

```xml
<PackageReference Include="Microsoft.Graph" />
<PackageReference Include="Microsoft.Kiota.Abstractions" />
```

## Creating a Graph client

```csharp
// IGraphClientFactory implementation
public GraphServiceClient CreateClient(string accessToken)
    => new(new BaseBearerTokenAuthenticationProvider(
        new StaticAccessTokenProvider(accessToken)));

private sealed class StaticAccessTokenProvider(string token) : IAccessTokenProvider
{
    public Task<string> GetAuthorizationTokenAsync(Uri uri, ..., CancellationToken ct = default)
        => Task.FromResult(token);

    public AllowedHostsValidator AllowedHostsValidator { get; } = new(["graph.microsoft.com"]);
}
```

- Create a **new client per access token** — never cache the client, only the drive context (see below).
- `IGraphClientFactory` is registered as a singleton. Inject it where Graph access is needed.

## Drive context caching

Every Graph call that targets a specific drive needs two IDs: `DriveId` and the root item ID (`RootId`). Fetching them is two API calls. Cache them in-memory keyed on `accountId`:

```csharp
// Fetch once, cache forever (until eviction on sign-out)
var drive = await client.Me.Drive.GetAsync(ct);
var root  = await client.Drives[drive.Id].Root.GetAsync(ct);
_cache[accountId] = new DriveContext(new DriveId(drive.Id), root.Id);
```

- Call `EvictCachedDriveContext(accountId)` on sign-out or when a `403`/`401` is received.
- All private helpers that need the drive context call `ResolveClientWithDriveContextAsync`, which returns `Result<(GraphServiceClient Client, DriveContext Ctx), GraphError>`.

## Getting root folders

```csharp
var response = await client
    .Drives[driveId].Items[rootId].Children
    .GetAsync(req => req.QueryParameters.Select = ["id", "name", "folder", "file", "size",
        "lastModifiedDateTime", "parentReference", "eTag", "cTag",
        "@microsoft.graph.downloadUrl"], ct);
```

- Filter to `item.Folder is not null` to get folders only.
- **Always** paginate: loop on `OdataNextLink` using `.WithUrl(nextLink).GetAsync(ct)`.
- Return `DriveFolder(Id, Name, ParentId)` records ordered by name.

## Getting child folders (lazy folder tree)

```csharp
var result = await client
    .Drives[driveId].Items[parentFolderId].Children
    .GetAsync(req =>
    {
        req.QueryParameters.Select = ["id", "name", "folder", "parentReference"];
        req.QueryParameters.Top = 100;
    }, ct);
```

- Same pagination loop as root folders.
- Used for expanding nodes in the folder picker tree.

## Getting drive quota

```csharp
var drive = await client
    .Drives[driveId]
    .GetAsync(req => req.QueryParameters.Select = ["quota"], ct);
long total = drive?.Quota?.Total ?? 0L;
long used  = drive?.Quota?.Used  ?? 0L;
```

## Full folder enumeration (sync)

Recursively enumerates a folder subtree. Use a `HashSet<string>` of visited IDs to prevent cycles:

```csharp
static async Task EnumerateSubFolderAsync(GraphServiceClient client, DriveId driveId, string parentId,
    string relativePath, List<DeltaItem> items, HashSet<string> visited, CancellationToken ct)
{
    if (!visited.Add(parentId)) return;  // cycle guard

    var page = await client.Drives[driveId.Value].Items[parentId].Children
        .GetAsync(req => req.QueryParameters.Select = _childrenSelect, ct);

    while (page?.Value is not null)
    {
        foreach (var item in page.Value)
        {
            string itemPath = BuildRelativePath(relativePath, item);
            items.Add(MapToDeltaItem(item, itemPath));
            if (item.Folder is not null && item.Id is not null)
                await EnumerateSubFolderAsync(client, driveId, item.Id, itemPath, items, visited, ct);
        }
        if (page.OdataNextLink is null) break;
        page = await client.Drives[driveId.Value].Items[parentId].Children
            .WithUrl(page.OdataNextLink).GetAsync(ct: ct);
    }
}
```

Returns `Result<List<DeltaItem>, GraphError>`.

## Getting an item by path

```csharp
var item = await client
    .Drives[driveId].Items[$"root:/{remotePath}"]
    .GetAsync(req => req.QueryParameters.Select = ["id"], ct);
// Returns null on 404 — catch ApiException with ResponseStatusCode == 404
```

## Getting a download URL

Select `@microsoft.graph.downloadUrl` and read from `item.AdditionalData`:

```csharp
var item = await client.Drives[driveId].Items[itemId]
    .GetAsync(req => req.QueryParameters.Select = ["@microsoft.graph.downloadUrl"], ct);

if (!item.AdditionalData.TryGetValue("@microsoft.graph.downloadUrl", out var url) || url is null)
    return new Result<string, GraphError>.Error(GraphErrorFactory.NotFound(itemId));

return new Result<string, GraphError>.Ok(url.ToString()!);
```

Download URLs are **pre-signed** and expire. Do not cache them.

## Uploading a file

Always use resumable upload sessions — even for small files (simpler unified path):

```csharp
// 1. Create upload session
var session = await client
    .Drives[driveId].Items[parentFolderId]
    .ItemWithPath(remotePath)
    .CreateUploadSession
    .PostAsync(requestBody, ct);

// requestBody sets @microsoft.graph.conflictBehavior = "replace"
// and fileSystemInfo.lastModifiedDateTime from local file's LastWriteTimeUtc

// 2. PUT chunks to session.UploadUrl
```

See `onedrive-sync.md` for the full upload/retry protocol.

## Deleting an item

```csharp
await client.Drives[driveId].Items[itemId].DeleteAsync(ct: ct);
```

Returns `Result<Unit, GraphError>`.

## DeltaItem mapping

Map `DriveItem` → `DeltaItem` discriminated union:

- `FileDeltaItem` — when `item.Folder is null`
- `FolderDeltaItem` — when `item.Folder is not null`
- `DeletedDeltaItem` — when item has `Deleted` facet (delta query only)

All nullable Graph fields map to `Option<T>`:

```csharp
var parentId = item.ParentReference?.Id is string pid
    ? Option.Some(new OneDriveFolderId(pid))
    : Option.None<OneDriveFolderId>();

var lastModified = item.LastModifiedDateTime.ToOption();  // extension method
var downloadUrl  = ExtractDownloadUrl(item);              // Option<string>
```

## IGraphService contract

```csharp
Task<Result<DriveId, GraphError>>             GetDriveIdAsync(string accountId, string accessToken, CancellationToken ct = default);
Task<Result<List<DriveFolder>, GraphError>>   GetRootFoldersAsync(string accountId, string accessToken, CancellationToken ct = default);
Task<Result<List<DriveFolder>, GraphError>>   GetChildFoldersAsync(string accessToken, DriveId driveId, string parentFolderId, CancellationToken ct = default);
Task<Result<(long Total, long Used), GraphError>> GetQuotaAsync(string accountId, string accessToken, CancellationToken ct = default);
Task<Result<List<DeltaItem>, GraphError>>   EnumerateFolderAsync(string accessToken, DriveId driveId, string folderId, string remotePath, CancellationToken ct = default);
Task<Option<string>>                        GetFolderIdByPathAsync(string accessToken, DriveId driveId, string remotePath, CancellationToken ct = default);
Task<Result<string, GraphError>>            GetDownloadUrlAsync(string accountId, string accessToken, string itemId, CancellationToken ct = default);
Task<Result<string, GraphError>>            UploadFileAsync(string accountId, string accessToken, string localPath, string remotePath, string parentFolderId, CancellationToken ct = default);
Task<Result<Unit, GraphError>>              DeleteItemAsync(string accountId, string accessToken, string itemId, CancellationToken ct = default);
void                                      EvictCachedDriveContext(string accountId);
```

## Result wrapping rule

**Every** Graph API call returns `Result<T, GraphError>` or `Option<T>`. Never `string` as error type. The caller uses `Map` / `Bind` / `Match` / `MatchAsync` — never `is Ok` pattern matching. See `@.claude/rules/functional-usage.md` for the full rules.
