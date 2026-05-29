namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Associates a set of keywords with a <see cref="FileClassification"/> for path-based classification.</summary>
/// <param name="Classification">The classification to assign when a keyword matches.</param>
/// <param name="Keywords">The keywords to check against the remote path.</param>
public sealed record FileClassificationRule(FileClassification Classification, IReadOnlyList<string> Keywords);
