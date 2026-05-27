namespace AStar.Dev.CloudSyncFunctional.Onboarding;

/// <summary>Base type for persistence errors.</summary>
public abstract record PersistenceError
{
    /// <summary>Gets the human-readable error message.</summary>
    public abstract string Message { get; }
}

/// <summary>An unexpected error occurred during a persistence operation.</summary>
public sealed record PersistenceUnexpectedError : PersistenceError
{
    /// <inheritdoc/>
    public override string Message { get; }

    /// <summary>Initialises a new <see cref="PersistenceUnexpectedError"/> with the given message.</summary>
    /// <param name="message">The error message.</param>
    public PersistenceUnexpectedError(string message) => Message = message;
}
