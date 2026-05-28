using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using GraphClient = Microsoft.Graph.GraphServiceClient;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <inheritdoc />
public sealed partial class GraphService(IGraphClientFactory clientFactory, ILogger<GraphService> logger) : IGraphService
{
    private static readonly string[] ChildrenSelect = ["id", "name", "folder", "parentReference"];

    /// <inheritdoc />
    public Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersAsync(string accountId, string accessToken, CancellationToken cancellationToken = default)
        => ExecuteGraphOperationAsync(
            accountId,
            () => GetClientAndDriveAsync(clientFactory, accessToken, cancellationToken)
                .BindAsync(clientAndDrive => GetRootFoldersForDriveAsync(clientAndDrive.Client, clientAndDrive.DriveFound, cancellationToken)));

    private async Task<Result<T, GraphError>> ExecuteGraphOperationAsync<T>(string accountId, Func<Task<Result<T, GraphError>>> operation)
    {
        try
        {
            return await operation().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogGraphFailed(logger, accountId, ex.Message);

            return new Fail<T, GraphError>(GraphErrorFactory.Unexpected(ex.Message));
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
            ? await children.GetAsync(req => req.QueryParameters.Select = ChildrenSelect, cancellationToken: cancellationToken).ConfigureAwait(false)
            : await children.WithUrl(nextLink).GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return page is { }
            ? new Ok<DriveItemCollectionResponse, GraphError>(page)
            : new Fail<DriveItemCollectionResponse, GraphError>(GraphErrorFactory.Unexpected("Folder page was null."));
    }

    private static Task<Result<List<DriveFolder>, GraphError>> GetFoldersFromPagesAsync(GraphClient client, DriveFound driveFound, RootFound rootFound, DriveItemCollectionResponse page, IReadOnlyCollection<DriveFolder>? foldersSoFar, CancellationToken cancellationToken)
    {
        var folders = (foldersSoFar ?? []).Concat(GetFoldersFromPage(page)).ToList();

        return page.OdataNextLink is null
            ? Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>(folders))
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

    [LoggerMessage(Level = LogLevel.Error, Message = "Graph API call failed for account {AccountId}: {ErrorMessage}")]
    private static partial void LogGraphFailed(ILogger logger, string accountId, string errorMessage);

    private sealed record ClientAndDriveFound(GraphClient Client, DriveFound DriveFound);
}
