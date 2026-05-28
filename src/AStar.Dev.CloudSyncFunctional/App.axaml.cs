using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Persistence;
using AStar.Dev.CloudSyncFunctional.Persistence.Repositories;
using AStar.Dev.CloudSyncFunctional.Recovery;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.CloudSyncFunctional.Sync.Pipeline;
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
using System.IO.Abstractions;
using Testably.Abstractions;
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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = _serviceProvider.GetRequiredService<WorkspaceViewModel>();
            desktop.MainWindow = new MainWindow(viewModel);
            _ = InitialiseAsync(_serviceProvider, viewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(MELogLevel.Debug));
        services.AddHttpClient();

        var clientId = configuration["MicrosoftIdentity:ClientId"]
            ?? throw new InvalidOperationException("MicrosoftIdentity:ClientId is not configured. Set it in appsettings.json or user secrets.");

        services.AddSingleton(_ =>
            PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority("https://login.microsoftonline.com/consumers")
                .WithRedirectUri("http://localhost")
                .Build());

        services.AddSingleton<ITokenCacheService, TokenCacheService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IGraphClientFactory, GraphClientFactory>();
        services.AddSingleton<IGraphService, GraphService>();

        var fileSystem = new RealFileSystem();
        services.AddSingleton<IFileSystem>(fileSystem);

        var connectionString = $"DataSource={GetDatabasePath(fileSystem)}";
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(connectionString), ServiceLifetime.Singleton);

        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<ISyncRuleRepository, SyncRuleRepository>();
        services.AddTransient<ISyncedItemRepository, SyncedItemRepository>();
        services.AddTransient<IDriveStateRepository, DriveStateRepository>();
        services.AddTransient<ISyncRepository, SyncRepository>();
        services.AddTransient<IFileClassificationRuleRepository, FileClassificationRuleRepository>();

        services.AddTransient<IAccountOnboardingService, AccountOnboardingService>();
        services.AddTransient<ISyncRecoveryService, SyncRecoveryService>();

        services.AddSingleton<IHttpDownloader, HttpDownloader>();
        services.AddSingleton<IUploadService, UploadService>();
        services.AddSingleton<ISyncWorkerFactory, SyncWorkerFactory>();
        services.AddSingleton<ISyncPipeline, SyncPipeline>();
        services.AddSingleton<IRemoteFolderEnumerator, RemoteFolderEnumerator>();
        services.AddSingleton<IRemoteDeletionDetector, RemoteDeletionDetector>();
        services.AddSingleton<ILocalDeletionDetector, LocalDeletionDetector>();
        services.AddSingleton<IDownloadJobBuilder, DownloadJobBuilder>();
        services.AddSingleton<ILocalChangeDetector, LocalChangeDetector>();
        services.AddSingleton<IJobExecutor, JobExecutor>();
        services.AddSingleton<ISyncService, SyncService>();
        services.AddSingleton<ISyncScheduler, SyncScheduler>();

        services.AddTransient<AddAccountWizardViewModel>();
        services.AddTransient<WorkspaceViewModel>();
    }

    private static async Task InitialiseAsync(IServiceProvider serviceProvider, WorkspaceViewModel viewModel)
    {
        await ApplyDatabaseMigrationsAsync(serviceProvider);
        await viewModel.LoadPersistedAccountsAsync(CancellationToken.None);
    }

    private static async Task ApplyDatabaseMigrationsAsync(IServiceProvider serviceProvider)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var context = await dbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    private static string GetDatabasePath(IFileSystem fileSystem)
    {
        var configDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = fileSystem.Path.Combine(configDir, "astar-dev-cloudsync");
        fileSystem.Directory.CreateDirectory(appDir);

        return fileSystem.Path.Combine(appDir, "sync.db");
    }
}
