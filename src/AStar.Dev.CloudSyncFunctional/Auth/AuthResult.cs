namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>The result of a successful authentication with Microsoft.</summary>
/// <param name="AccessToken">The OAuth2 access token for Graph API calls.</param>
/// <param name="AccountId">The MSAL HomeAccountId identifier.</param>
/// <param name="Profile">The authenticated user's profile.</param>
/// <param name="ExpiresOn">When the access token expires.</param>
public sealed record AuthResult(string AccessToken, string AccountId, AccountProfile Profile, DateTimeOffset ExpiresOn);

/// <summary>Creates <see cref="AuthResult"/> instances with validated inputs.</summary>
public static class AuthResultFactory
{
    /// <summary>Creates a validated <see cref="AuthResult"/>.</summary>
    /// <param name="accessToken">The OAuth2 access token.</param>
    /// <param name="accountId">The MSAL account identifier.</param>
    /// <param name="profile">The user profile.</param>
    /// <param name="expiresOn">The token expiry time.</param>
    /// <returns>A new <see cref="AuthResult"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="accessToken"/> or <paramref name="accountId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profile"/> is null.</exception>
    public static AuthResult Create(string accessToken, string accountId, AccountProfile profile, DateTimeOffset expiresOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId);
        ArgumentNullException.ThrowIfNull(profile);

        return new AuthResult(accessToken, accountId, profile, expiresOn);
    }
}
