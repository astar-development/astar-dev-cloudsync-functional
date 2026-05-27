namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>Indicates whether a sync rule includes or excludes a path.</summary>
public enum RuleType
{
    /// <summary>The path is included in sync.</summary>
    Include,

    /// <summary>The path is excluded from sync.</summary>
    Exclude
}
