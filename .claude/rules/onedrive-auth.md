# OneDrive Authentication (MSAL)

This repo uses `Microsoft.Identity.Client` (MSAL) for OAuth2 authentication against personal Microsoft accounts via the Microsoft Graph API.

## Packages required

```xml
<PackageReference Include="Microsoft.Identity.Client" />
<PackageReference Include="Microsoft.Identity.Client.Extensions.Msal" />
```

## Building the `IPublicClientApplication`

```csharp
var app = PublicClientApplicationBuilder
    .Create(clientId)                                  // Entra App Registration ClientId
    .WithAuthority("https://login.microsoftonline.com/consumers")
    .WithRedirectUri("http://localhost")                // loopback — works on Linux, Windows, macOS without WebView2
    .WithClientName(ApplicationMetadata.ApplicationName)
    .WithClientVersion(version)
    .Build();
```

- **Always** use `WithUseEmbeddedWebView(false)` on `AcquireTokenInteractive` — forces system browser, which works cross-platform.
- **Always** use `WithPrompt(Prompt.SelectAccount)` on interactive sign-in — prevents silent re-use of cached accounts.

## Scopes

Minimum required scopes:

```
Files.ReadWrite      — read/write files in OneDrive
offline_access       — get refresh tokens (app works without re-auth between sessions)
User.Read            — get display name and email from profile
```

## Token acquisition

### Interactive sign-in (new account)

```csharp
var result = await app
    .AcquireTokenInteractive(scopes)
    .WithPrompt(Prompt.SelectAccount)
    .WithUseEmbeddedWebView(false)
    .ExecuteAsync(ct);
```

Return type: `Result<AuthResult, AuthError>`. Catch and map:

| Exception | Maps to |
|---|---|
| `MsalClientException` with `ErrorCode` == `"authentication_canceled"` / `"user_canceled"` | `AuthCancelledError` |
| `OperationCanceledException` | `AuthCancelledError` |
| `MsalException` | `AuthFailedError(ex.Message)` |
| Any other `Exception` | `AuthFailedError(ex.Message)` |

### Silent token refresh (existing account)

```csharp
var accounts = await app.GetAccountsAsync();
var account = accounts.FirstOrDefault(a => a.HomeAccountId?.Identifier == accountId);
if (account is null) return AuthResultFactory.Failure("Account not found in token cache.");

var result = await app
    .AcquireTokenSilent(scopes, account)
    .ExecuteAsync(ct);
```

- If `MsalUiRequiredException` is thrown: return `AuthFailedError("Re-authentication required.")` — the caller must trigger interactive sign-in.

### Token expiry check

Before making a Graph API call, check whether the current access token expires within 5 minutes. If so, call `AcquireTokenSilentAsync` to get a fresh token — MSAL uses the refresh token transparently.

```csharp
private static bool TokenNeedsRefresh(DateTimeOffset expiresOn)
    => expiresOn - DateTimeOffset.UtcNow < TimeSpan.FromMinutes(5);
```

`AuthResult` must expose `ExpiresOn` (type `DateTimeOffset`) so callers can check before use. Services that hold an access token always call `AcquireTokenSilentAsync` at the point of use rather than caching the token string — MSAL handles renewal internally. Never hold an access token across a long-running operation without re-acquiring it.

### Sign-out

```csharp
var accounts = await app.GetAccountsAsync();
var account = accounts.FirstOrDefault(a => a.HomeAccountId?.Identifier == accountId);
if (account is not null)
    await app.RemoveAsync(account);
```

## Multi-account

- All service methods that need a token take **both** `accountId` (string — the MSAL `HomeAccountId.Identifier`) and `accessToken` (string — the bearer token for Graph calls).
- Token cache is keyed on `accountId`. The `IPublicClientApplication` instance is shared across all accounts.
- **Never** use `app.GetAccountsAsync()` to pick an account inside `GraphService` — that belongs in `AuthService` only.

## Token cache persistence (`ITokenCacheService`)

Use `Microsoft.Identity.Client.Extensions.Msal.MsalCacheHelper` to register a file-backed cache **once** after building the app.

### Platform cache locations

| Platform | Path |
|---|---|
| Linux | `~/.config/<app-name-hyphenated>/` |
| Windows | `%AppData%\<app-name-hyphenated>\` |
| macOS | `~/Library/Application Support/<app-name-hyphenated>/` |

### Linux keyring pattern

```csharp
// attempt keyring first; fall back to plaintext on failure
var keyringProperties = new StorageCreationPropertiesBuilder(cacheFileName, cacheDir)
    .WithLinuxKeyring(schemaName: "dev.astar.onedrivesync", ...)
    .Build();

// 5-second timeout on keyring — libsecret can hang in headless environments
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var helper = await MsalCacheHelper.CreateAsync(keyringProperties).WaitAsync(cts.Token);
helper.RegisterCache(app.UserTokenCache);
```

If keyring registration throws, fall back to `.WithLinuxUnprotectedFile()` and log a warning. Never crash the app because keyring is unavailable.

### Registration guard

```csharp
private bool _cacheRegistered;

private async Task EnsureCacheRegisteredAsync()
{
    if (_cacheRegistered) return;
    await _cacheService.RegisterAsync(_app);
    _cacheRegistered = true;
}
```

Call `EnsureCacheRegisteredAsync` at the top of every `AuthService` method — never register multiple times.

## Profile extraction

After interactive sign-in, extract display name and email from `ClaimsPrincipal` first (claims `"name"`, `"preferred_username"`, `"email"`), falling back to `account.Username`:

```csharp
string displayName = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? result.Account.Username;
string email = result.ClaimsPrincipal?.FindFirst("preferred_username")?.Value
               ?? result.ClaimsPrincipal?.FindFirst("email")?.Value
               ?? result.Account.Username;
```

## AuthResult domain types

- `AuthResult` — record: `AccessToken`, `AccountId`, `Profile`
- `AuthError` — discriminated union: `AuthCancelledError`, `AuthFailedError(string Message)`
- Use `AuthResultFactory` for construction. Never construct directly.

## IAuthService contract

```csharp
Task<Result<AuthResult, AuthError>> SignInInteractiveAsync(CancellationToken ct = default);
Task<Result<AuthResult, AuthError>> AcquireTokenSilentAsync(string accountId, CancellationToken ct = default);
Task SignOutAsync(string accountId, CancellationToken ct = default);
Task<IReadOnlyList<string>> GetCachedAccountIdsAsync();
```
