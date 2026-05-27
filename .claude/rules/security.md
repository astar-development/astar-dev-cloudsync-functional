# Security Rules

## No hardcoded secrets — ever

Client IDs, client secrets, API keys, connection strings, and any other credential **must never appear in source code or committed files**. This includes placeholder GUIDs such as `00000000-0000-0000-0000-000000000000` — a placeholder is still a hardcoded value and must not be committed.

### Configuration pattern for this project

Secrets are loaded via `IConfiguration` in `App.axaml.cs` at startup. The chain is:

1. `appsettings.json` — committed to source control; contains only non-secret defaults and placeholder values
2. User secrets (`secrets.json`) — machine-local, never committed; overrides `appsettings.json` at development time

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddUserSecrets<App>()
    .Build();
```

Read secrets from configuration, never from a literal:

```csharp
// ❌ banned
.Create("a1b2c3d4-real-client-id")

// ✅ correct
var clientId = configuration["MicrosoftIdentity:ClientId"]
    ?? throw new InvalidOperationException("MicrosoftIdentity:ClientId is not configured.");
.Create(clientId)
```

### Setting a real value locally

```bash
dotnet user-secrets set "MicrosoftIdentity:ClientId" "<your-real-client-id>" \
  --project src/AStar.Dev.CloudSyncFunctional
```

User secrets are stored at `~/.microsoft/usersecrets/astar-dev-cloudsync-functional/secrets.json` and are never committed to git.

### Configuration keys

| Key | Where set | Purpose |
|---|---|---|
| `MicrosoftIdentity:ClientId` | User secrets | Entra App Registration client ID |

### What is allowed in `appsettings.json`

- Non-secret configuration (authority URL, redirect URI, log level)
- Placeholder values (`"00000000-0000-0000-0000-000000000000"`) that make the shape of config obvious — **as long as the real value is never committed**

### Code review checklist

Before approving any PR that touches DI registration or configuration:

- [ ] No real GUIDs, tokens, or keys in any `.cs`, `.json`, or `.axaml` file
- [ ] Any new secret has a corresponding `configuration["Key"]` read with a `?? throw`
- [ ] `appsettings.json` contains only placeholder or non-secret values
- [ ] New configuration keys are documented in the table above
