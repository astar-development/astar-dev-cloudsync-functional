using Microsoft.Graph;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>Creates Graph SDK clients authenticated with a given access token.</summary>
public interface IGraphClientFactory
{
    /// <summary>Creates a new <see cref="GraphServiceClient"/> authenticated with the given bearer token.</summary>
    /// <param name="accessToken">The OAuth2 bearer token.</param>
    /// <returns>A new <see cref="GraphServiceClient"/> instance.</returns>
    GraphServiceClient CreateClient(string accessToken);
}
