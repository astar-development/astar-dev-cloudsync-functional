using AStar.Dev.CloudSyncFunctional.Persistence.Entities;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Represents a sync rule that includes or excludes a remote path from sync.</summary>
/// <param name="RemotePath">The remote OneDrive path this rule applies to.</param>
/// <param name="RuleType">Whether the path is included or excluded from sync.</param>
public sealed record SyncRule(string RemotePath, RuleType RuleType);

/// <summary>Creates <see cref="SyncRule"/> instances.</summary>
public static class SyncRuleFactory
{
    /// <summary>Creates a new include rule for the given path.</summary>
    /// <param name="remotePath">The remote OneDrive path to include in sync.</param>
    /// <returns>A <see cref="SyncRule"/> with <see cref="RuleType.Include"/>.</returns>
    public static SyncRule CreateInclude(string remotePath) => new(remotePath, RuleType.Include);

    /// <summary>Creates a new exclude rule for the given path.</summary>
    /// <param name="remotePath">The remote OneDrive path to exclude from sync.</param>
    /// <returns>A <see cref="SyncRule"/> with <see cref="RuleType.Exclude"/>.</returns>
    public static SyncRule CreateExclude(string remotePath) => new(remotePath, RuleType.Exclude);
}
