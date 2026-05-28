namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Classifies remote paths against a set of <see cref="FileClassificationRule"/> instances.</summary>
public static class FileClassifier
{
    /// <summary>Classifies the given remote path against the provided rules.</summary>
    /// <remarks>
    /// Each rule's keywords are checked against the full remote path using a case-insensitive contains check.
    /// All matching classifications are returned. When no rules match, a single unclassified result is returned.
    /// </remarks>
    /// <param name="remotePath">The remote OneDrive path to classify.</param>
    /// <param name="rules">The classification rules to evaluate.</param>
    /// <returns>All matching classifications, or a single <see cref="FileClassificationFactory.CreateUnclassified"/> result when no rules match.</returns>
    public static IReadOnlyList<FileClassification> Classify(string remotePath, IReadOnlyList<FileClassificationRule> rules)
    {
        var matches = rules
            .Where(rule => rule.Keywords.Any(keyword => remotePath.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Select(rule => rule.Classification)
            .ToList();

        return matches.Count > 0 ? matches : [FileClassificationFactory.CreateUnclassified()];
    }
}
