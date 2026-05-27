namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for a file classification rule.</summary>
public sealed class FileClassificationRuleEntity
{
    /// <summary>Gets or sets the rule identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable name for this rule.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the classification label applied when this rule matches.</summary>
    public string Classification { get; set; } = string.Empty;

    /// <summary>Gets or sets the keywords that trigger this rule, stored as a comma-delimited string.</summary>
    public string Keywords { get; set; } = string.Empty;
}
