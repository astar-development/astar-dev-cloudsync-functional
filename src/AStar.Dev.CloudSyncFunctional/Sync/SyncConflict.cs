using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Represents the resolution state of a sync conflict.</summary>
public enum ConflictState
{
    /// <summary>The conflict has not yet been resolved.</summary>
    Pending,

    /// <summary>The conflict has been resolved.</summary>
    Resolved
}

/// <summary>The policy to apply when resolving a sync conflict.</summary>
public enum ConflictPolicy
{
    /// <summary>Discard the remote version; re-upload the local version.</summary>
    KeepLocal,

    /// <summary>Discard the local version; download the remote version.</summary>
    KeepRemote,

    /// <summary>Leave both versions untouched and mark the conflict as unresolved.</summary>
    Skip
}

/// <summary>Represents a detected sync conflict between a local and remote file version.</summary>
/// <param name="Id">The conflict identifier.</param>
/// <param name="AccountId">The account identifier this conflict belongs to.</param>
/// <param name="RemoteItemId">The Graph item identifier of the conflicting remote file.</param>
/// <param name="LocalModifiedAt">The local file's last-modified timestamp at the time the conflict was detected.</param>
/// <param name="RemoteModifiedAt">The remote file's last-modified timestamp at the time the conflict was detected.</param>
/// <param name="State">The current resolution state of the conflict.</param>
public sealed record SyncConflict(SyncConflictId Id, string AccountId, string RemoteItemId, DateTimeOffset LocalModifiedAt, DateTimeOffset RemoteModifiedAt, ConflictState State);
