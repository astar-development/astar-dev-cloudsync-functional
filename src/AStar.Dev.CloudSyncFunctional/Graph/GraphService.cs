using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <inheritdoc />
public sealed partial class GraphService(IGraphClientFactory clientFactory, ILogger<GraphService> logger) : IGraphService
{
    /// <inheritdoc />
    public Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersAsync(string accountId, string accessToken, CancellationToken ct = default)
        => throw new NotImplementedException();
}
