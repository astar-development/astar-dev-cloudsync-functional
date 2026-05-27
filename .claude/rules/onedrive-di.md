# Dependency Injection — Avalonia Desktop App

This app uses `Microsoft.Extensions.DependencyInjection`. All services are registered at startup and resolved via constructor injection. `new` is never used to construct a service.

## Packages required

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" />
<PackageReference Include="Microsoft.Extensions.Logging" />
<PackageReference Include="Microsoft.Extensions.Hosting" />
<PackageReference Include="Microsoft.Extensions.Http" />  <!-- IHttpClientFactory -->
<PackageReference Include="Serilog.Extensions.Hosting" />  <!-- UseSerilog() -->
<PackageReference Include="Serilog.Extensions.Logging" />  <!-- ILogger<T> bridge -->
```

## Lifetimes

Desktop apps have no HTTP request scope. Use only **Singleton** and **Transient**. `Scoped` is banned — there is no scope boundary in an Avalonia app.

### Singleton

One instance for the entire application lifetime. Use for services that own state, are expensive to initialise, or must be shared across the app:

| Service | Reason |
|---|---|
| `IPublicClientApplication` (MSAL) | Shared MSAL instance; owns the token cache |
| `IAuthService` | Wraps the single MSAL instance |
| `IGraphClientFactory` | Stateless factory |
| `IGraphService` | Manages per-account drive context cache |
| `ISyncScheduler` | Owns the timer and active sync state |
| `ISyncService` | Owns progress events subscribed to by multiple ViewModels |
| `ISyncPipeline` | Stateless parallel job runner |
| `ISyncWorkerFactory` | Stateless factory |
| `IAccountOnboardingService` | Stateless — orchestrates account + rule persistence on wizard completion |
| `ITokenCacheService` | Single file-backed token cache |
| `IFileSystem` | Testably abstraction — stateless |
| `IHttpClientFactory` | Registered via `services.AddHttpClient()` — manages handler lifetimes |
| `IHttpDownloader` | Stateless; obtains `HttpClient` from `IHttpClientFactory` per call |
| `IUploadService` | Stateless; obtains `HttpClient` from `IHttpClientFactory` per call |
| `ISyncedItemRegistrar` | Stateless; delegates to repository |
| `IDbContextFactory<AppDbContext>` | Factory used to create contexts per-operation |

### Transient

New instance every time. Use for lightweight services that create their own resources on each operation:

| Service | Reason |
|---|---|
| `IAccountRepository` | Creates its own `AppDbContext` per operation via `IDbContextFactory` |
| `ISyncRuleRepository` | Same |
| `ISyncedItemRepository` | Same |
| `IDriveStateRepository` | Same |
| `ISyncRepository` | Same |
| `IFileClassificationRuleRepository` | Same |
| `ISyncWorker` | Created per pipeline run by `ISyncWorkerFactory` — not registered directly |
| ViewModels | New instance per navigation |

## AppDbContext — factory pattern

Never inject `AppDbContext` directly. Register `IDbContextFactory<AppDbContext>` as Singleton and create contexts per operation:

```csharp
// Registration in startup
services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString), ServiceLifetime.Singleton);

// Repository usage — one context per async operation
public class AccountRepository(IDbContextFactory<AppDbContext> dbFactory) : IAccountRepository
{
    public async Task<Option<AccountEntity>> GetByIdAsync(AccountId id, CancellationToken cancellationToken)
    {
        await using var context = await dbFactory.CreateDbContextAsync(ct);
        var entity = await context.Accounts.FindAsync([id], ct);
        return entity is null ? new None<AccountEntity>() : new Some<AccountEntity>(entity);
    }
}
```

Never register `AppDbContext` as `Scoped` — desktop apps have no scope boundary.

## ViewModels

Register as Transient:

```csharp
services.AddTransient<MainWindowViewModel>();
services.AddTransient<AccountsPaneViewModel>();
services.AddTransient<AddAccountWizardViewModel>();
```

ViewModels receive dependencies via constructor injection. Never resolve ViewModels from the container inside other ViewModels — communicate via events or ReactiveUI observables instead.

## Wizard ViewModel lifetime

The `AddAccountWizardViewModel` is created at the point the wizard opens, not at app startup:

```csharp
// In the host ViewModel — resolve on demand, not at construction
private void OpenAddAccountWizard()
{
    var wizard = _serviceProvider.GetRequiredService<AddAccountWizardViewModel>();
    wizard.Completed += OnWizardCompleted;
    wizard.Cancelled += OnWizardCancelled;
    CurrentContent = wizard;
}
```

Dispose the ViewModel when the wizard closes. Do not hold a long-lived reference.

## Unhandled exceptions — top-level catch

Unexpected exceptions that escape infrastructure boundaries are caught at the application top level only. This is the single location outside infrastructure where exceptions are permitted:

```csharp
// In App.axaml.cs
TaskScheduler.UnobservedTaskException += (_, args) =>
{
    LogUnhandledException(_logger, args.Exception);
    args.SetObserved();
    RxApp.MainThreadScheduler.Schedule(() =>
    {
        _mainWindowViewModel.HasError = true;
        _mainWindowViewModel.ErrorMessage = "An unexpected error occurred. Please restart the application.";
    });
};
```

Log the full exception. Surface a user-facing message. Do not swallow.

## What is banned

- `Scoped` lifetime — no request scope in a desktop app
- Service locator pattern — never inject `IServiceProvider` into business logic
- `new` to construct any service
- Static service instances
- Resolving ViewModels at app startup (resolve on demand at the point of navigation)
