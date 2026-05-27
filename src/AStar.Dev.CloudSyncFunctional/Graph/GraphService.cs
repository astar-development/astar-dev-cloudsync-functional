using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <inheritdoc />
public sealed partial class GraphService(IGraphClientFactory clientFactory, ILogger<GraphService> logger) : IGraphService
{
    private static readonly string[] ChildrenSelect = ["id", "name", "folder", "parentReference"];

    /// <inheritdoc />
    public async Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersAsync(string accountId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = clientFactory.CreateClient(accessToken);
            var drive = await client.Me.Drive.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (drive?.Id is null)
                return new Fail<List<DriveFolder>, GraphError>(GraphErrorFactory.Unexpected("Drive ID was null."));

            var root = await client.Drives[drive.Id].Root.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (root?.Id is null)
                return new Fail<List<DriveFolder>, GraphError>(GraphErrorFactory.Unexpected("Root item ID was null."));

            var page = await client.Drives[drive.Id].Items[root.Id].Children
                .GetAsync(req => req.QueryParameters.Select = ChildrenSelect, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var folders = new List<DriveFolder>();
            while (page?.Value is not null)
            {
                foreach (var item in page.Value.Where(i => i.Folder is not null))
                    folders.Add(new DriveFolder(item.Id!, item.Name!, item.ParentReference?.Id));

                if (page.OdataNextLink is null)
                    break;

                page = await client.Drives[drive.Id].Items[root.Id].Children
                    .WithUrl(page.OdataNextLink).GetAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            return new Ok<List<DriveFolder>, GraphError>(folders);
        }
        catch (Exception ex)
        {
            LogGraphFailed(logger, accountId, ex.Message);

            return new Fail<List<DriveFolder>, GraphError>(GraphErrorFactory.Unexpected(ex.Message));
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Graph API call failed for account {AccountId}: {ErrorMessage}")]
    private static partial void LogGraphFailed(ILogger logger, string accountId, string errorMessage);
}
