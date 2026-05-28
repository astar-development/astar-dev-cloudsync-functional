using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Enumerates remote folders for all root-level include sync rules, deduplicating ancestor paths.</summary>
public sealed partial class RemoteFolderEnumerator(IGraphService graphService, ILogger<RemoteFolderEnumerator> logger) : IRemoteFolderEnumerator
{
    /// <summary>Enumerates all remote items across the account's configured include rules.</summary>
    /// <remarks>
    /// Only root-level include rules are enumerated. A rule is a root rule if no other include rule is a path ancestor of it.
    /// This prevents double-enumeration when both a parent and child folder are selected.
    /// </remarks>
    /// <param name="account">The account to enumerate remote items for.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="rules">The sync rules for the account.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All delta items found in the configured remote folders, or a <see cref="SyncError"/> on failure.</returns>
    public async Task<Result<List<DeltaItem>, SyncError>> EnumerateAsync(OneDriveAccount account, string accessToken, IReadOnlyList<SyncRule> rules, CancellationToken cancellationToken)
    {
        var includeRules = rules.Where(rule => rule.RuleType == RuleType.Include).ToList();
        var rootRules = includeRules.Where(rule => !includeRules.Any(other => other.RemotePath != rule.RemotePath && IsAncestor(other.RemotePath, rule.RemotePath))).ToList();

        if (rootRules.Count == 0)
            return new Fail<List<DeltaItem>, SyncError>(SyncErrorFactory.NoFoldersConfigured());

        var allItems = new List<DeltaItem>();
        foreach (var rule in rootRules)
        {
            var error = await ProcessRuleAsync(rule, allItems, account, accessToken, cancellationToken).ConfigureAwait(false);
            if (error is not null)
                return new Fail<List<DeltaItem>, SyncError>(error);
        }

        return new Ok<List<DeltaItem>, SyncError>(allItems);
    }

    private Task<SyncError?> ProcessRuleAsync(SyncRule rule, List<DeltaItem> allItems, OneDriveAccount account, string accessToken, CancellationToken cancellationToken)
        => graphService.GetFolderIdByPathAsync(accessToken, account.DriveIdValue, rule.RemotePath.TrimStart('/'), cancellationToken)
            .MatchAsync(
                async folderId => await graphService.EnumerateFolderAsync(accessToken, account.DriveIdValue, folderId, rule.RemotePath, cancellationToken)
                    .MatchAsync<List<DeltaItem>, GraphError, SyncError?>(
                        items => { allItems.AddRange(items); return (SyncError?)null; },
                        error =>
                        {
                            LogEnumerationFailed(logger, rule.RemotePath, error.Message);

                            return (SyncError?)SyncErrorFactory.GraphFailed(error);
                        }),
                _ =>
                {
                    LogFolderNotFound(logger, rule.RemotePath);

                    return Task.FromResult<SyncError?>(null);
                });

    private static bool IsAncestor(string potentialAncestor, string path)
    {
        if (!path.StartsWith(potentialAncestor, StringComparison.OrdinalIgnoreCase))
            return false;

        if (path.Length == potentialAncestor.Length)
            return false;

        return path[potentialAncestor.Length] == '/';
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Folder not found for path {RemotePath} — skipping")]
    private static partial void LogFolderNotFound(ILogger logger, string remotePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Enumeration failed for path {RemotePath}: {ErrorMessage}")]
    private static partial void LogEnumerationFailed(ILogger logger, string remotePath, string errorMessage);
}
