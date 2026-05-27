using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <inheritdoc />
public sealed partial class AuthService(IPublicClientApplication app, ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    public Task<Result<AuthResult, AuthError>> SignInInteractiveAsync(CancellationToken ct = default)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public Task<Result<AuthResult, AuthError>> AcquireTokenSilentAsync(string accountId, CancellationToken ct = default)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public Task SignOutAsync(string accountId, CancellationToken ct = default)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetCachedAccountIdsAsync()
        => throw new NotImplementedException();
}
