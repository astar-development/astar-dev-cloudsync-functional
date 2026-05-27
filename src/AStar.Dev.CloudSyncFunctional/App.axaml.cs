using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Wizard;
using AStar.Dev.CloudSyncFunctional.Workspace;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MELogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AStar.Dev.CloudSyncFunctional;

/// <summary>Application entry point and DI composition root.</summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    /// <inheritdoc/>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddUserSecrets<App>()
            .Build();

        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        _serviceProvider = services.BuildServiceProvider();

        ApplyDatabaseMigrations(_serviceProvider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow(_serviceProvider.GetRequiredService<WorkspaceViewModel>());

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(MELogLevel.Debug));

        var clientId = configuration["MicrosoftIdentity:ClientId"]
            ?? throw new InvalidOperationException("MicrosoftIdentity:ClientId is not configured. Set it in appsettings.json or user secrets.");

        services.AddSingleton<IPublicClientApplication>(_ =>
            PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority("https://login.microsoftonline.com/consumers")
                .WithRedirectUri("http://localhost")
                .Build());

        services.AddSingleton<ITokenCacheService, TokenCacheService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGraphClientFactory, GraphClientFactory>();
        services.AddSingleton<IGraphService, GraphService>();

        var connectionString = $"DataSource={GetDatabasePath()}";
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(connectionString), ServiceLifetime.Singleton);

        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<ISyncRuleRepository, SyncRuleRepository>();
        services.AddTransient<ISyncedItemRepository, SyncedItemRepository>();
        services.AddTransient<IDriveStateRepository, DriveStateRepository>();
        services.AddTransient<ISyncRepository, SyncRepository>();
        services.AddTransient<IFileClassificationRuleRepository, FileClassificationRuleRepository>();

        services.AddTransient<IAccountOnboardingService, AccountOnboardingService>();
        services.AddTransient<AddAccountWizardViewModel>();
        services.AddTransient<WorkspaceViewModel>();
    }

    private static void ApplyDatabaseMigrations(IServiceProvider serviceProvider)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var startupContext = dbContextFactory.CreateDbContext();
        startupContext.Database.Migrate();
    }

    private static string GetDatabasePath()
    {
        var configDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = Path.Combine(configDir, "astar-dev-cloudsync");
        Directory.CreateDirectory(appDir);

        return Path.Combine(appDir, "sync.db");
    }
}
