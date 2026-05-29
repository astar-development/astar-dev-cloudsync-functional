namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>The current state of the sync operation.</summary>
public enum SyncState
{
    /// <summary>No sync is in progress.</summary>
    Idle,

    /// <summary>A sync is actively running.</summary>
    Syncing,

    /// <summary>The sync encountered an error.</summary>
    Error
}

/// <summary>Carries progress information for a running sync operation.</summary>
/// <param name="AccountId">The account identifier the sync is running for.</param>
/// <param name="CurrentFile">The remote path of the file currently being processed.</param>
/// <param name="Completed">The number of jobs completed so far.</param>
/// <param name="Total">The total number of jobs in this sync pass.</param>
/// <param name="StatusMessage">A human-readable summary of the current stage.</param>
/// <param name="State">The current sync state.</param>
public sealed record SyncProgressEventArgs(string AccountId, string CurrentFile, int Completed, int Total, string StatusMessage, SyncState State);
