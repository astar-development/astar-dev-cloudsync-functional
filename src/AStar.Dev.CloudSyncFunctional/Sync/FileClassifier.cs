namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Classifies remote paths against a set of <see cref="FileClassificationRule"/> instances.</summary>
public static class FileClassifier
{
    private static readonly char[] PathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '-', '_', '.', ' '];

    /// <summary>Classifies the given remote path against the provided rules.</summary>
    /// <remarks>
    /// The path is tokenised on / - _ . (space) and each token is checked against rule keywords using a case-insensitive
    /// exact-equality comparison. A token must equal a keyword exactly (ignoring case) to match; substrings do not count.
    /// All matching classifications are returned. When no rules match, a single unclassified result is returned.
    /// </remarks>
    /// <param name="remotePath">The remote OneDrive path to classify.</param>
    /// <param name="rules">The classification rules to evaluate.</param>
    /// <returns>All matching classifications, or a single <see cref="FileClassificationFactory.CreateUnclassified"/> result when no rules match.</returns>
    public static IReadOnlyList<FileClassification> Classify(string remotePath, IReadOnlyList<FileClassificationRule> rules)
    {
        var tokens = remotePath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
        var matches = rules
            .Where(rule => rule.Keywords.Any(kw => tokens.Any(t => t.Equals(kw, StringComparison.OrdinalIgnoreCase))))
            .Select(rule => rule.Classification)
            .ToList();

        return matches.Count > 0 ? matches : [FileClassificationFactory.CreateUnclassified()];
    }
}
