using System.IO.Abstractions;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed partial class HttpDownloader(IHttpClientFactory httpClientFactory, IFileSystem fileSystem, ILogger<HttpDownloader> logger) : IHttpDownloader
{
    private const int MaxRetries = 5;
    private const string UserAgent = "AStar.Dev.CloudSyncFunctional/1.0";

    /// <inheritdoc />
    public async Task<Result<Unit, SyncError>> DownloadAsync(string url, string localPath, DateTimeOffset remoteModified, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using var client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(ParseRetryAfter(response), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    LogDownloadFailed(logger, localPath, attempt, $"HTTP {(int)response.StatusCode}");
                    if (attempt == MaxRetries)
                        return new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(Graph.GraphErrorFactory.Unexpected($"HTTP {(int)response.StatusCode}")));

                    await Task.Delay(GetBackoffDelay(attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await WriteFileAsync(contentStream, localPath, remoteModified, progress, cancellationToken).ConfigureAwait(false);

                LogDownloadSucceeded(logger, localPath);

                return new Ok<Unit, SyncError>(Unit.Default);
            }
            catch (OperationCanceledException)
            {
                return new Fail<Unit, SyncError>(SyncErrorFactory.Cancelled());
            }
            catch (HttpRequestException ex)
            {
                LogDownloadFailed(logger, localPath, attempt, ex.Message);
                if (attempt == MaxRetries)
                    return new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(Graph.GraphErrorFactory.Network(ex.Message)));

                await Task.Delay(GetBackoffDelay(attempt), cancellationToken).ConfigureAwait(false);
            }
        }

        return new Fail<Unit, SyncError>(SyncErrorFactory.GraphFailed(Graph.GraphErrorFactory.Unexpected("Max retries exceeded.")));
    }

    private async Task WriteFileAsync(Stream contentStream, string localPath, DateTimeOffset remoteModified, IProgress<long>? progress, CancellationToken cancellationToken)
    {
        var directory = fileSystem.Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory) && !fileSystem.Directory.Exists(directory))
            fileSystem.Directory.CreateDirectory(directory);

        await using var fileStream = fileSystem.File.Open(localPath, FileMode.Create, FileAccess.Write);

        var buffer = new byte[81920];
        long totalBytesWritten = 0;
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesWritten += bytesRead;
            progress?.Report(totalBytesWritten);
        }

        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        fileStream.Close();

        fileSystem.File.SetCreationTime(localPath, remoteModified.LocalDateTime);
        fileSystem.File.SetLastWriteTime(localPath, remoteModified.LocalDateTime);
    }

    private static TimeSpan ParseRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Delta is TimeSpan delta)
            return delta;

        if (response.Headers.RetryAfter?.Date is DateTimeOffset date)
            return date - DateTimeOffset.UtcNow;

        return TimeSpan.FromSeconds(10);
    }

    private static TimeSpan GetBackoffDelay(int attempt)
    {
        var seconds = Math.Min(2.0 * Math.Pow(2, attempt - 1), 120.0);
        var jitter = seconds * 0.2 * Random.Shared.NextDouble();

        return TimeSpan.FromSeconds(seconds + jitter);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Downloaded {LocalPath}")]
    private static partial void LogDownloadSucceeded(ILogger logger, string localPath);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Download attempt {Attempt} failed for {LocalPath}: {ErrorMessage}")]
    private static partial void LogDownloadFailed(ILogger logger, string localPath, int attempt, string errorMessage);
}
