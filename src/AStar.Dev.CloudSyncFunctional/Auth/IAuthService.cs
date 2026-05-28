using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>Provides MSAL-based authentication for Microsoft accounts.</summary>
public interface IAuthService
{
    /// <summary>Opens an interactive browser sign-in and returns the authenticated result.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResult"/> on success, or an <see cref="AuthError"/> on failure.</returns>
    Task<Result<AuthResult, AuthError>> SignInInteractiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Silently acquires a new access token for an existing cached account.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResult"/> on success, or an <see cref="AuthError"/> on failure.</returns>
    Task<Result<AuthResult, AuthError>> AcquireTokenSilentAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>Signs out and removes the account from the token cache.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when sign-out is done.</returns>
    Task SignOutAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>Returns the list of account IDs currently in the token cache.</summary>
    /// <returns>A read-only list of MSAL HomeAccountId identifiers.</returns>
    Task<IReadOnlyList<string>> GetCachedAccountIdsAsync();
}
