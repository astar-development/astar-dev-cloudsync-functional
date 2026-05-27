using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>Provides access to OneDrive via the Microsoft Graph API.</summary>
public interface IGraphService
{
    /// <summary>Returns the root-level folders for the given account's OneDrive.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    /// <returns>A list of root folders, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersAsync(string accountId, string accessToken, CancellationToken ct = default);
}
