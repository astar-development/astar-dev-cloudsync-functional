using System.Collections.Concurrent;
using System.IO.Abstractions;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using GraphClient = Microsoft.Graph.GraphServiceClient;
using PersistenceDriveId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.DriveId;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <inheritdoc />
public sealed partial class GraphService(IGraphClientFactory clientFactory, IHttpClientFactory httpClientFactory, IFileSystem fileSystem, ILogger<GraphService> logger) : IGraphService
{
    private static readonly string[] RootChildrenSelect = ["id", "name", "folder", "parentReference"];
    private static readonly string[] EnumerateChildrenSelect = ["id", "name", "folder", "parentReference", "lastModifiedDateTime", "file", "eTag"];
    private const int ChunkSize = 10 * 1024 * 1024;

    private readonly ConcurrentDictionary<string, string> _driveIdCache = new();

    /// <inheritdoc />
    public Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersAsync(string accountId, string accessToken, CancellationToken cancellationToken = default)
        => ExecuteGraphOperationAsync(
            accountId,
            () => GetClientAndDriveAsync(clientFactory, accessToken, cancellationToken)
                .BindAsync(clientAndDrive => GetRootFoldersForDriveAsync(clientAndDrive.Client, clientAndDrive.DriveFound, cancellationToken)));

    /// <inheritdoc />
    public Task<Result<List<DeltaItem>, GraphError>> EnumerateFolderAsync(string accessToken, PersistenceDriveId driveId, string folderId, string remotePath, CancellationToken cancellationToken = default)
        => ExecuteGraphOperationAsync(
            folderId,
            () => TryGetClientForToken(clientFactory, accessToken)
                .BindAsync(async client =>
                {
                    var items = new List<DeltaItem>();
                    var visited = new HashSet<string>();
                    await EnumerateSubFolderAsync(client, driveId, folderId, remotePath, items, visited, cancellationToken).ConfigureAwait(false);

                    return (Result<List<DeltaItem>, GraphError>)new Ok<List<DeltaItem>, GraphError>(items);
                }));

    /// <inheritdoc />
    public async Task<Option<string>> GetFolderIdByPathAsync(string accessToken, PersistenceDriveId driveId, string remotePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return await TryGetClientForToken(clientFactory, accessToken)
                .BindAsync(async client =>
                {
                    var item = await client.Drives[driveId.Value].Items[$"root:/{remotePath}"]
                        .GetAsync(req => req.QueryParameters.Select = ["id"], cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return item?.Id is string id
                        ? (Result<Option<string>, GraphError>)new Ok<Option<string>, GraphError>(new Some<string>(id))
                        : new Ok<Option<string>, GraphError>(new None<string>());
                })
                .MatchAsync<Option<string>, GraphError, Option<string>>(opt => opt, _ => new None<string>());
        }
        catch (ODataError)
        {
            return new None<string>();
        }
        catch (Exception ex)
        {
            LogGraphFailed(logger, remotePath, ex.Message);

            return new None<string>();
        }
    }

    /// <inheritdoc />
    public Task<Result<string, GraphError>> GetDownloadUrlAsync(string accountId, string accessToken, string itemId, CancellationToken cancellationToken = default)
        => ExecuteGraphOperationAsync(
            accountId,
            () => TryGetClientForToken(clientFactory, accessToken)
                .BindAsync(async client =>
                {
                    var driveId = await GetOrResolveDriveIdAsync(accountId, client, cancellationToken).ConfigureAwait(false);
                    if (driveId is null)
                        return (Result<string, GraphError>)new Fail<string, GraphError>(GraphErrorFactory.Unexpected("Could not resolve drive ID."));

                    var item = await client.Drives[driveId].Items[itemId]
                        .GetAsync(req => req.QueryParameters.Select = ["@microsoft.graph.downloadUrl"], cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    if (item?.AdditionalData is null || !item.AdditionalData.TryGetValue("@microsoft.graph.downloadUrl", out var url) || url is null)
                        return (Result<string, GraphError>)new Fail<string, GraphError>(GraphErrorFactory.NotFound(itemId));

                    return (Result<string, GraphError>)new Ok<string, GraphError>(url.ToString()!);
                }));

    /// <inheritdoc />
    public Task<Result<string, GraphError>> UploadFileAsync(string accountId, string accessToken, string localPath, string remotePath, string parentFolderId, CancellationToken cancellationToken = default)
        => ExecuteGraphOperationAsync(
            accountId,
            () => TryGetClientForToken(clientFactory, accessToken)
                .BindAsync(async client =>
                {
                    var driveId = await GetOrResolveDriveIdAsync(accountId, client, cancellationToken).ConfigureAwait(false);
                    if (driveId is null)
                        return (Result<string, GraphError>)new Fail<string, GraphError>(GraphErrorFactory.Unexpected("Could not resolve drive ID for upload."));

                    var fileName = fileSystem.Path.GetFileName(localPath);
                    var lastModified = fileSystem.File.GetLastWriteTimeUtc(localPath)
                        .ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);

                    var sessionBody = new Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession.CreateUploadSessionPostRequestBody
                    {
                        Item = new DriveItemUploadableProperties
                        {
                            AdditionalData = new Dictionary<string, object>
                            {
                                { "@microsoft.graph.conflictBehavior", "replace" },
                                { "name", fileName },
                                { "fileSystemInfo", new Dictionary<string, object> { { "lastModifiedDateTime", lastModified } } }
                            }
                        }
                    };

                    var session = await client.Drives[driveId].Items[parentFolderId]
                        .ItemWithPath(fileName)
                        .CreateUploadSession
                        .PostAsync(sessionBody, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    if (session?.UploadUrl is null)
                        return (Result<string, GraphError>)new Fail<string, GraphError>(GraphErrorFactory.Unexpected("Upload session URL was null."));

                    await using var fileStream = fileSystem.File.OpenRead(localPath);

                    return await UploadChunksAsync(session.UploadUrl, fileStream, fileStream.Length, cancellationToken).ConfigureAwait(false);
                }));

    /// <inheritdoc />
    public Task<Result<Unit, GraphError>> DeleteItemAsync(string accountId, string accessToken, string itemId, CancellationToken cancellationToken = default)
        => ExecuteGraphOperationAsync(
            accountId,
            () => TryGetClientForToken(clientFactory, accessToken)
                .BindAsync(async client =>
                {
                    var driveId = await GetOrResolveDriveIdAsync(accountId, client, cancellationToken).ConfigureAwait(false);
                    if (driveId is null)
                        return (Result<Unit, GraphError>)new Fail<Unit, GraphError>(GraphErrorFactory.Unexpected("Could not resolve drive ID for delete."));

                    await client.Drives[driveId].Items[itemId].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                    return (Result<Unit, GraphError>)new Ok<Unit, GraphError>(Unit.Default);
                }));

    /// <inheritdoc />
    public void EvictCachedDriveContext(string accountId) => _driveIdCache.TryRemove(accountId, out _);

    private async Task<string?> GetOrResolveDriveIdAsync(string accountId, GraphClient client, CancellationToken cancellationToken)
    {
        if (_driveIdCache.TryGetValue(accountId, out var cached))
            return cached;

        var drive = await client.Me.Drive.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (drive?.Id is null)
            return null;

        _driveIdCache[accountId] = drive.Id;

        return drive.Id;
    }

    private async Task<Result<T, GraphError>> ExecuteGraphOperationAsync<T>(string accountId, Func<Task<Result<T, GraphError>>> operation)
    {
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogGraphFailed(logger, accountId, ex.Message);

            return GraphErrorFactory.Unexpected(ex.Message);
        }
    }

    private static Task<Result<ClientAndDriveFound, GraphError>> GetClientAndDriveAsync(IGraphClientFactory clientFactory, string accessToken, CancellationToken cancellationToken)
        => clientFactory.CreateClient(accessToken)
            .BindAsync(client => GetDriveAsync(client, cancellationToken)
                .MapAsync(driveFound => new ClientAndDriveFound(client, driveFound)));

    private static Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersForDriveAsync(GraphClient client, DriveFound driveFound, CancellationToken cancellationToken)
        => GetRootAsync(client, driveFound, cancellationToken)
            .BindAsync(rootFound => GetFolderPageAsync(client, driveFound, rootFound, null, cancellationToken)
                .BindAsync(firstPage => GetFoldersFromPagesAsync(client, driveFound, rootFound, firstPage, null, cancellationToken)));

    private static async Task<Result<DriveItemCollectionResponse, GraphError>> GetFolderPageAsync(GraphClient client, DriveFound driveFound, RootFound rootFound, string? nextLink, CancellationToken cancellationToken)
    {
        var children = client.Drives[driveFound.Drive.Id].Items[rootFound.DriveItem.Id].Children;
        var page = nextLink is null
            ? await children.GetAsync(req => req.QueryParameters.Select = RootChildrenSelect, cancellationToken: cancellationToken).ConfigureAwait(false)
            : await children.WithUrl(nextLink).GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return page is { }
            ? new Ok<DriveItemCollectionResponse, GraphError>(page)
            : new Fail<DriveItemCollectionResponse, GraphError>(GraphErrorFactory.Unexpected("Folder page was null."));
    }

    private static Task<Result<List<DriveFolder>, GraphError>> GetFoldersFromPagesAsync(GraphClient client, DriveFound driveFound, RootFound rootFound, DriveItemCollectionResponse page, IReadOnlyCollection<DriveFolder>? foldersSoFar, CancellationToken cancellationToken)
    {
        var folders = (foldersSoFar ?? []).Concat(GetFoldersFromPage(page)).ToList();

        return page.OdataNextLink is null
            ? Task.FromResult<Result<List<DriveFolder>, GraphError>>(folders)
            : GetFolderPageAsync(client, driveFound, rootFound, page.OdataNextLink, cancellationToken)
                .BindAsync(nextPage => GetFoldersFromPagesAsync(client, driveFound, rootFound, nextPage, folders, cancellationToken));
    }

    private static IEnumerable<DriveFolder> GetFoldersFromPage(DriveItemCollectionResponse page)
        => page.Value?
               .Where(item => item.Folder is not null)
               .Where(item => item.Id is not null && item.Name is not null)
               .Select(item => new DriveFolder(item.Id!, item.Name!, item.ParentReference?.Id))
           ?? [];

    private static async Task<Result<RootFound, GraphError>> GetRootAsync(GraphClient client, DriveFound driveFound, CancellationToken cancellationToken)
        => (await client.Drives[driveFound.Drive.Id].Root.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false)) is DriveItem { Id: not null } root
            ? new Ok<RootFound, GraphError>(new RootFound(root))
            : new Fail<RootFound, GraphError>(GraphErrorFactory.Unexpected("Root item was null."));

    private static async Task<Result<DriveFound, GraphError>> GetDriveAsync(GraphClient client, CancellationToken cancellationToken)
        => (await client.Me.Drive.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false)) is { Id: not null } drive
            ? new Ok<DriveFound, GraphError>(new DriveFound(drive))
            : new Fail<DriveFound, GraphError>(GraphErrorFactory.Unexpected("Drive was null."));

    private static Result<GraphClient, GraphError> TryGetClientForToken(IGraphClientFactory factory, string accessToken)
        => factory.CreateClient(accessToken);

    private static async Task EnumerateSubFolderAsync(GraphClient client, PersistenceDriveId driveId, string parentId, string relativePath, List<DeltaItem> items, HashSet<string> visited, CancellationToken cancellationToken)
    {
        if (!visited.Add(parentId))
            return;

        DriveItemCollectionResponse? page = await client.Drives[driveId.Value].Items[parentId].Children
            .GetAsync(req => req.QueryParameters.Select = EnumerateChildrenSelect, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        while (page?.Value is not null)
        {
            foreach (var item in page.Value)
            {
                if (item.Id is null || item.Name is null)
                    continue;

                var itemPath = BuildRelativePath(relativePath, item.Name);
                if (item.Folder is not null)
                {
                    items.Add(new FolderDeltaItem(item.Id, item.Name, itemPath));
                    await EnumerateSubFolderAsync(client, driveId, item.Id, itemPath, items, visited, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    items.Add(new FileDeltaItem(item.Id, item.Name, itemPath, item.ETag, item.LastModifiedDateTime));
                }
            }

            if (page.OdataNextLink is null)
                break;

            page = await client.Drives[driveId.Value].Items[parentId].Children
                .WithUrl(page.OdataNextLink)
                .GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static string BuildRelativePath(string parentPath, string itemName)
        => string.IsNullOrEmpty(parentPath) ? itemName : $"{parentPath}/{itemName}";

    private async Task<Result<string, GraphError>> UploadChunksAsync(string uploadUrl, Stream fileStream, long totalSize, CancellationToken cancellationToken)
    {
        const int maxRetries = 5;
        var buffer = new byte[ChunkSize];
        long offset = 0;
        using var httpClient = httpClientFactory.CreateClient("Graph");

        while (offset < totalSize)
        {
            var bytesRead = await fileStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
                break;

            var end = offset + bytesRead - 1;
            Result<string, GraphError>? chunkCompleted = null;

            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var content = new System.Net.Http.ByteArrayContent(buffer, 0, bytesRead);
                content.Headers.TryAddWithoutValidation("Content-Range", $"bytes {offset}-{end}/{totalSize}");
                content.Headers.ContentLength = bytesRead;

                var response = await httpClient.PutAsync(uploadUrl, content, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    var itemId = doc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                    chunkCompleted = itemId is not null
                        ? (Result<string, GraphError>)new Ok<string, GraphError>(itemId)
                        : new Fail<string, GraphError>(GraphErrorFactory.Unexpected("Upload completed but item ID was missing."));
                    break;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new Fail<string, GraphError>(GraphErrorFactory.Unexpected("Upload session expired (404). Restart required."));

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(ParseChunkRetryAfter(response), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    if (attempt == maxRetries)
                        return new Fail<string, GraphError>(GraphErrorFactory.Unexpected($"Upload chunk failed after {maxRetries} attempts: HTTP {(int)response.StatusCode}"));

                    await Task.Delay(GetChunkBackoffDelay(attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                break;
            }

            if (chunkCompleted is not null)
                return chunkCompleted;

            offset += bytesRead;
        }

        return new Fail<string, GraphError>(GraphErrorFactory.Unexpected("Upload completed without receiving item ID."));
    }

    private static TimeSpan ParseChunkRetryAfter(System.Net.Http.HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Delta is TimeSpan delta)
            return delta;

        if (response.Headers.RetryAfter?.Date is DateTimeOffset date)
            return date - DateTimeOffset.UtcNow;

        return TimeSpan.FromSeconds(10);
    }

    private static TimeSpan GetChunkBackoffDelay(int attempt)
    {
        var seconds = Math.Min(2.0 * Math.Pow(2, attempt - 1), 120.0);
        var jitter = seconds * 0.2 * Random.Shared.NextDouble();

        return TimeSpan.FromSeconds(seconds + jitter);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Graph API call failed for account {AccountId}: {ErrorMessage}")]
    private static partial void LogGraphFailed(ILogger logger, string accountId, string errorMessage);

    private sealed record ClientAndDriveFound(GraphClient Client, DriveFound DriveFound);
}
