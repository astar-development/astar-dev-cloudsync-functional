using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using MELogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <inheritdoc />
public sealed partial class TokenCacheService(ILogger<TokenCacheService> logger) : ITokenCacheService
{
    private const string CacheFileName = "token_cache.bin3";
    private const string AppFolderName = "astar-cloudsync";

    private static readonly string CacheDir = BuildCacheDir();

    /// <inheritdoc />
    public async Task RegisterAsync(IPublicClientApplication app, CancellationToken cancellationToken = default)
    {
        try
        {
            var properties = BuildProperties();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var helper = await MsalCacheHelper.CreateAsync(properties).WaitAsync(cts.Token).ConfigureAwait(false);
            helper.RegisterCache(app.UserTokenCache);
        }
        catch (Exception ex) when (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            LogKeyringUnavailable(logger, ex.Message);
            await RegisterWithUnprotectedFileAsync(app).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogCacheRegistrationFailed(logger, ex.Message);
        }
    }

    private async Task RegisterWithUnprotectedFileAsync(IPublicClientApplication app)
    {
        try
        {
            var properties = new StorageCreationPropertiesBuilder(CacheFileName, CacheDir)
                .WithLinuxUnprotectedFile()
                .Build();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var helper = await MsalCacheHelper.CreateAsync(properties).WaitAsync(cts.Token).ConfigureAwait(false);
            helper.RegisterCache(app.UserTokenCache);
        }
        catch (Exception ex)
        {
            LogCacheRegistrationFailed(logger, ex.Message);
        }
    }

    private static StorageCreationProperties BuildProperties()
    {
        var builder = new StorageCreationPropertiesBuilder(CacheFileName, CacheDir);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            builder = builder.WithLinuxKeyring(
                "dev.astar.onedrivesync",
                "default",
                "MSALCache",
                new KeyValuePair<string, string>("Version", "1"),
                new KeyValuePair<string, string>("Product", "AStar.CloudSync"));

        return builder.Build();
    }

    private static string BuildCacheDir()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Path.Combine(home, "AppData", "Roaming", AppFolderName);

        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? Path.Combine(home, "Library", "Application Support", AppFolderName)
            : Path.Combine(home, ".config", AppFolderName);
    }

    [LoggerMessage(Level = MELogLevel.Warning, Message = "Linux keyring unavailable, falling back to unprotected file: {ErrorMessage}")]
    private static partial void LogKeyringUnavailable(ILogger logger, string errorMessage);

    [LoggerMessage(Level = MELogLevel.Warning, Message = "Token cache registration failed: {ErrorMessage}")]
    private static partial void LogCacheRegistrationFailed(ILogger logger, string errorMessage);
}
