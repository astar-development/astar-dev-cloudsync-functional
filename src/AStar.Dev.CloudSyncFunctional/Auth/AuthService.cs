using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MELogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <inheritdoc />
public sealed partial class AuthService(IPublicClientApplication app, ILogger<AuthService> logger) : IAuthService
{
    private static readonly string[] Scopes = ["Files.ReadWrite", "offline_access", "User.Read"];

    /// <inheritdoc />
    public async Task<Result<AuthResult, AuthError>> SignInInteractiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var msalResult = await app
                .AcquireTokenInteractive(Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .WithUseEmbeddedWebView(false)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            return new Ok<AuthResult, AuthError>(BuildAuthResult(msalResult));
        }
        catch (MsalClientException ex) when (ex.ErrorCode is "authentication_canceled" or "user_canceled")
        {
            return new Fail<AuthResult, AuthError>(AuthErrorFactory.Cancelled());
        }
        catch (OperationCanceledException)
        {
            return new Fail<AuthResult, AuthError>(AuthErrorFactory.Cancelled());
        }
        catch (MsalException ex)
        {
            LogAuthFailed(logger, ex.Message);
            return new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            LogAuthFailed(logger, ex.Message);
            return new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed(ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<AuthResult, AuthError>> AcquireTokenSilentAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            var account = accounts.FirstOrDefault(a => a.HomeAccountId?.Identifier == accountId);
            if (account is null)
                return new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed("Account not found in token cache."));

            var msalResult = await app.AcquireTokenSilent(Scopes, account).ExecuteAsync(cancellationToken).ConfigureAwait(false);

            return new Ok<AuthResult, AuthError>(BuildAuthResult(msalResult));
        }
        catch (MsalUiRequiredException)
        {
            return new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed("Re-authentication required."));
        }
        catch (Exception ex)
        {
            LogAuthFailed(logger, ex.Message);
            return new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed(ex.Message));
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
