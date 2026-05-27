using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>Sync configuration stored as a complex property on <see cref="AccountEntity"/>.</summary>
public sealed class AccountSyncConfig
{
    /// <summary>Gets or sets the local folder where files are synced.</summary>
    public LocalSyncPath LocalSyncPath { get; set; }

    /// <summary>Gets or sets the number of parallel sync workers (1–10).</summary>
    public int WorkerCount { get; set; } = 8;
}
