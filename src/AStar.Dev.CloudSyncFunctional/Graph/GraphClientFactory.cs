using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <inheritdoc />
public sealed class GraphClientFactory : IGraphClientFactory
{
    /// <inheritdoc />
    public GraphServiceClient CreateClient(string accessToken) =>
        new(new BaseBearerTokenAuthenticationProvider(new StaticAccessTokenProvider(accessToken)));

    private sealed class StaticAccessTokenProvider(string token) : IAccessTokenProvider
    {
        /// <summary>Returns the static bearer token for every request.</summary>
        /// <param name="uri">The request URI (unused).</param>
        /// <param name="additionalAuthenticationContext">Additional context (unused).</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The access token.</returns>
        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(token);

        /// <inheritdoc />
        public AllowedHostsValidator AllowedHostsValidator { get; } = new(["graph.microsoft.com"]);
    }
}
