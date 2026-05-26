# Account Onboarding

`IAccountOnboardingService` is called when the add-account wizard completes. It owns all persistence and configuration steps that follow a successful sign-in and folder selection.

## IAccountOnboardingService contract

```csharp
/// <summary>
/// Persists a new account and its initial sync configuration after the wizard completes.
/// Returns the finalised <see cref="OneDriveAccount"/> with all defaults applied.
/// </summary>
Task<Result<OneDriveAccount, PersistenceError>> CompleteOnboardingAsync(OneDriveAccount account, CancellationToken ct = default);
```

## Responsibilities (in order)

1. **Resolve default sync path** — if no `LocalSyncPath` is configured, compute the default (see below) and set it on the account.
2. **Persist the account** — upsert via `IAccountRepository`.
3. **Write sync rules** — for each folder the user selected, upsert a `SyncRuleEntity` with `RuleType.Include`.
4. **Mark account active** — set `IsActive = true` before the final upsert.

## Default local sync path

When the user has not configured a local path, compute it as:

```
<user-home>/OneDrive/<sanitised-email>
```

| Platform | `<user-home>` |
|---|---|
| Linux | `/home/<username>` |
| Windows | `C:\Users\<username>` |
| macOS | `/Users/<username>` |

Use `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)` for the home directory — never hard-code paths.

### Email sanitisation

Strip any character that is invalid on any of the three supported OSes before using the email as a directory name. Characters to remove: `\ / : * ? " < > |` and all control characters (code points 0x00–0x1F).

```csharp
private static readonly char[] InvalidPathChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|',
    ..Enumerable.Range(0, 32).Select(i => (char)i)];

private static string SanitiseEmail(string email)
    => string.Concat(email.Where(c => !InvalidPathChars.Contains(c)));
```

Use `IFileSystem` for all path construction and directory creation — never `System.IO` directly.

## Wizard integration

The wizard ViewModel raises `Completed` with the draft `OneDriveAccount`. The host ViewModel calls `IAccountOnboardingService.CompleteOnboardingAsync` and handles the result via `MatchAsync`:

```csharp
wizard.Completed += async (_, account) =>
{
    await _onboardingService.CompleteOnboardingAsync(account, ct)
        .MatchAsync(
            finalAccount => { /* add to accounts list, navigate away */ },
            error        => { HasError = true; ErrorMessage = error.Message; });
};
```

Never perform persistence inside the wizard ViewModel itself.
