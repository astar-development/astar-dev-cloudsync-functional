namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Represents a file classification category with associated keywords.</summary>
/// <param name="Name">The name of the classification category.</param>
/// <param name="Keywords">The keywords associated with this classification.</param>
public sealed record FileClassification(string Name, IReadOnlyList<string> Keywords);

/// <summary>Creates <see cref="FileClassification"/> instances.</summary>
public static class FileClassificationFactory
{
    /// <summary>Creates the sentinel unclassified classification returned when no rules match.</summary>
    /// <returns>A classification with the name "Unclassified" and no keywords.</returns>
    public static FileClassification CreateUnclassified() => new("Unclassified", []);
}
