using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for a sync rule.</summary>
public sealed class SyncRuleEntity
{
    /// <summary>Gets or sets the sync rule identifier.</summary>
    public SyncRuleId Id { get; set; }

    /// <summary>Gets or sets the account this rule belongs to.</summary>
    public AccountId AccountId { get; set; }

    /// <summary>Gets or sets the remote OneDrive path this rule applies to.</summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this rule includes or excludes the path.</summary>
    public RuleType RuleType { get; set; }
}
