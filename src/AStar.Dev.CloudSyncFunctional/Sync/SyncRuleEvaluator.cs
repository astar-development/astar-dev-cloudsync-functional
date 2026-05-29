using AStar.Dev.CloudSyncFunctional.Persistence.Entities;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Evaluates sync rules to determine whether a remote path should be included or excluded.</summary>
public static class SyncRuleEvaluator
{
    /// <summary>Determines whether the given remote path is included by the set of sync rules.</summary>
    /// <remarks>
    /// The most-specific matching rule (longest prefix) wins. When two rules tie on length, Exclude wins.
    /// A path with no matching rules is excluded by default (default-deny).
    /// </remarks>
    /// <param name="remotePath">The remote OneDrive path to evaluate.</param>
    /// <param name="rules">The complete set of sync rules to evaluate against.</param>
    /// <returns><c>true</c> if the path is included; <c>false</c> if excluded or no rule matches.</returns>
    public static bool IsIncluded(string remotePath, IReadOnlyList<SyncRule> rules)
    {
        var matchingRules = rules
            .Where(rule => IsPrefix(rule.RemotePath, remotePath))
            .ToList();

        if (matchingRules.Count == 0)
            return false;

        var maxLength = matchingRules.Max(rule => rule.RemotePath.Length);
        var bestRules = matchingRules.Where(rule => rule.RemotePath.Length == maxLength).ToList();

        return bestRules.All(rule => rule.RuleType == RuleType.Include);
    }

    private static bool IsPrefix(string prefix, string path)
    {
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        if (path.Length == prefix.Length)
            return true;

        return path[prefix.Length] == '/';
    }
}
