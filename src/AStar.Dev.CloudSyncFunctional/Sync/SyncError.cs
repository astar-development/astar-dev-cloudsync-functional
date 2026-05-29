using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Base type for sync pipeline errors.</summary>
public abstract record SyncError
{
    /// <summary>Gets the human-readable error message.</summary>
    public abstract string Message { get; }
}

/// <summary>A sync error caused by an authentication failure.</summary>
public sealed record SyncAuthError(AuthError Inner) : SyncError
{
    /// <inheritdoc/>
    public override string Message => Inner.Message;
}

/// <summary>A sync error caused by a Graph API failure.</summary>
public sealed record SyncGraphError(GraphError Inner) : SyncError
{
    /// <inheritdoc/>
    public override string Message => Inner.Message;
}

/// <summary>A sync error caused by a persistence failure.</summary>
public sealed record SyncStorageError(PersistenceError Inner) : SyncError
{
    /// <inheritdoc/>
    public override string Message => Inner.Message;
}

/// <summary>No folders have been configured for sync.</summary>
public sealed record NoFoldersConfiguredError : SyncError
{
    /// <inheritdoc/>
    public override string Message => "No folders have been configured for sync.";
}

/// <summary>The sync operation was cancelled.</summary>
public sealed record SyncCancelledError : SyncError
{
    /// <inheritdoc/>
    public override string Message => "Sync was cancelled.";
}

/// <summary>Creates <see cref="SyncError"/> instances.</summary>
public static class SyncErrorFactory
{
    /// <summary>Creates a <see cref="SyncAuthError"/> wrapping the given authentication error.</summary>
    /// <param name="inner">The underlying authentication error.</param>
    /// <returns>A sync error wrapping the auth failure.</returns>
    public static SyncError AuthFailed(AuthError inner) => new SyncAuthError(inner);

    /// <summary>Creates a <see cref="SyncGraphError"/> wrapping the given Graph API error.</summary>
    /// <param name="inner">The underlying Graph error.</param>
    /// <returns>A sync error wrapping the Graph failure.</returns>
    public static SyncError GraphFailed(GraphError inner) => new SyncGraphError(inner);

    /// <summary>Creates a <see cref="SyncStorageError"/> wrapping the given persistence error.</summary>
    /// <param name="inner">The underlying persistence error.</param>
    /// <returns>A sync error wrapping the storage failure.</returns>
    public static SyncError StorageFailed(PersistenceError inner) => new SyncStorageError(inner);

    /// <summary>Creates a <see cref="NoFoldersConfiguredError"/>.</summary>
    /// <returns>An error indicating no folders are configured for sync.</returns>
    public static SyncError NoFoldersConfigured() => new NoFoldersConfiguredError();

    /// <summary>Creates a <see cref="SyncCancelledError"/>.</summary>
    /// <returns>An error indicating the sync was cancelled.</returns>
    public static SyncError Cancelled() => new SyncCancelledError();
}
