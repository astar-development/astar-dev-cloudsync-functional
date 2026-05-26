using AStar.Dev.CloudSyncFunctional;
using Avalonia;
using ReactiveUI.Avalonia;
using System;

namespace AStar.Dev.CloudSyncFunctional;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI(_ => { })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
