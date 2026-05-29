using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using PersistenceAccountId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.AccountId;

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
public sealed record SyncConflict(SyncConflictId Id, PersistenceAccountId AccountId, OneDriveItemId RemoteItemId, DateTimeOffset LocalModifiedAt, DateTimeOffset RemoteModifiedAt, ConflictState State);

/// <summary>Creates <see cref="SyncConflict"/> instances.</summary>
public static class SyncConflictFactory
{
    /// <summary>Creates a new <see cref="SyncConflict"/> in the <see cref="ConflictState.Pending"/> state.</summary>
    /// <param name="id">The conflict identifier.</param>
    /// <param name="accountId">The account identifier this conflict belongs to.</param>
    /// <param name="remoteItemId">The Graph item identifier of the conflicting remote file.</param>
    /// <param name="localModifiedAt">The local file's last-modified timestamp.</param>
    /// <param name="remoteModifiedAt">The remote file's last-modified timestamp.</param>
    /// <returns>A new pending <see cref="SyncConflict"/>.</returns>
    public static SyncConflict CreatePending(SyncConflictId id, PersistenceAccountId accountId, OneDriveItemId remoteItemId, DateTimeOffset localModifiedAt, DateTimeOffset remoteModifiedAt)
        => new(id, accountId, remoteItemId, localModifiedAt, remoteModifiedAt, ConflictState.Pending);
}
