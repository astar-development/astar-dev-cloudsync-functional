using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>EF Core persistence entity for a pending or completed sync job.</summary>
public sealed class SyncJobEntity
{
    /// <summary>Gets or sets the sync job identifier.</summary>
    public SyncJobId Id { get; set; }

    /// <summary>Gets or sets the account this job belongs to.</summary>
    public AccountId AccountId { get; set; }

    /// <summary>Gets or sets the remote OneDrive path involved in this job.</summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the local file system path involved in this job.</summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of job — "Download" or "Upload".</summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status — "Pending", "Running", "Completed", or "Failed".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when this job was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
