using Microsoft.Identity.Client;

namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>Registers the MSAL token cache with a persistent backing store.</summary>
public interface ITokenCacheService
{
    /// <summary>Registers the cache helper with the given MSAL application.</summary>
    /// <param name="app">The MSAL application whose token cache is registered.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when registration is done.</returns>
    Task RegisterAsync(IPublicClientApplication app, CancellationToken cancellationToken = default);
}
