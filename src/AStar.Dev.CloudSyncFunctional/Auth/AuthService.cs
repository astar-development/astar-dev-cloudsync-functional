using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MELogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <inheritdoc />
public sealed partial class AuthService(IPublicClientApplication app, ILogger<AuthService> logger, ITokenCacheService tokenCacheService) : IAuthService
{
    private static readonly string[] Scopes = ["Files.ReadWrite", "offline_access", "User.Read"];
    private bool _cacheRegistered;

    /// <inheritdoc />
    public async Task<Result<AuthResult, AuthError>> SignInInteractiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureCacheRegisteredAsync(cancellationToken).ConfigureAwait(false);

            var msalResult = await app
                .AcquireTokenInteractive(Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            return BuildAuthResult(msalResult);
        }
        catch (MsalClientException ex) when (ex.ErrorCode is "authentication_canceled" or "user_canceled")
        {
            return AuthErrorFactory.Cancelled();
        }
        catch (OperationCanceledException)
        {
            return AuthErrorFactory.Cancelled();
        }
        catch (MsalException ex)
        {
            LogAuthFailed(logger, ex.Message);

            return AuthErrorFactory.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            LogAuthFailed(logger, ex.Message);

            return AuthErrorFactory.Failed(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<Result<AuthResult, AuthError>> AcquireTokenSilentAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureCacheRegisteredAsync(cancellationToken).ConfigureAwait(false);

            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            var account = accounts.FirstOrDefault(a => a.HomeAccountId?.Identifier == accountId);
            if (account is null)
                return AuthErrorFactory.Failed("Account not found in token cache.");

            var msalResult = await app.AcquireTokenSilent(Scopes, account).ExecuteAsync(cancellationToken).ConfigureAwait(false);

            return BuildAuthResult(msalResult);
        }
        catch (MsalUiRequiredException)
        {
            LogAuthFailed(logger, "Re-authentication required.");

            return AuthErrorFactory.Failed("Re-authentication required.");
        }
        catch (Exception ex)
        {
            LogAuthFailed(logger, ex.Message);

            return AuthErrorFactory.Failed(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task SignOutAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        var account = accounts.FirstOrDefault(a => a.HomeAccountId?.Identifier == accountId);
        if (account is not null)
            await app.RemoveAsync(account).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetCachedAccountIdsAsync()
    {
        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);

        return [.. accounts.Where(a => a.HomeAccountId is not null).Select(a => a.HomeAccountId!.Identifier)];
    }

    private async Task EnsureCacheRegisteredAsync(CancellationToken cancellationToken)
    {
        if (_cacheRegistered) return;
        await tokenCacheService.RegisterAsync(app, cancellationToken).ConfigureAwait(false);
        _cacheRegistered = true;
    }

    private static AuthResult BuildAuthResult(AuthenticationResult result)
    {
        var displayName = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? result.Account.Username;
        var email = result.ClaimsPrincipal?.FindFirst("preferred_username")?.Value
                    ?? result.ClaimsPrincipal?.FindFirst("email")?.Value
                    ?? result.Account.Username;

        return AuthResultFactory.Create(
            result.AccessToken,
            result.Account.HomeAccountId.Identifier,
            AccountProfileFactory.Create(displayName, email),
            result.ExpiresOn);
    }

    [LoggerMessage(Level = MELogLevel.Error, Message = "Authentication failed: {ErrorMessage}")]
    private static partial void LogAuthFailed(ILogger logger, string errorMessage);
}
