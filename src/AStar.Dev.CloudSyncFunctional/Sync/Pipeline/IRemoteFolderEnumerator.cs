using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync.Pipeline;

/// <summary>Enumerates remote folders for all root-level include sync rules, deduplicating ancestor paths.</summary>
public interface IRemoteFolderEnumerator
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
    Task<Result<List<DeltaItem>, SyncError>> EnumerateAsync(OneDriveAccount account, string accessToken, IReadOnlyList<SyncRule> rules, CancellationToken cancellationToken);
}
