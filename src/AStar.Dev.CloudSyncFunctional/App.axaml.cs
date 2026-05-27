using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Wizard;
using AStar.Dev.CloudSyncFunctional.Workspace;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
        services.AddSingleton<IAccountOnboardingService, AccountOnboardingService>();
        services.AddTransient<AddAccountWizardViewModel>();
        services.AddTransient<WorkspaceViewModel>();
    }
}
