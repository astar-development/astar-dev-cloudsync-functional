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

/// <summary>A concurrency conflict occurred — the record was modified by another operation.</summary>
public sealed record ConcurrencyConflictError : PersistenceError
{
    /// <inheritdoc/>
    public override string Message => "A concurrency conflict occurred. The record was modified by another operation.";
}

/// <summary>A database constraint was violated during a persistence operation.</summary>
public sealed record ConstraintViolationError : PersistenceError
{
    /// <inheritdoc/>
    public override string Message { get; }

    /// <summary>Initialises a new <see cref="ConstraintViolationError"/> with the given detail.</summary>
    /// <param name="detail">A description of the constraint that was violated.</param>
    public ConstraintViolationError(string detail) => Message = detail;
}

/// <summary>Static factory for constructing <see cref="PersistenceError"/> instances.</summary>
public static class PersistenceErrorFactory
{
    /// <summary>Creates a <see cref="ConcurrencyConflictError"/>.</summary>
    /// <returns>A new concurrency conflict error.</returns>
    public static PersistenceError ConcurrencyConflict() => new ConcurrencyConflictError();

    /// <summary>Creates a <see cref="ConstraintViolationError"/>.</summary>
    /// <param name="detail">Optional detail describing the violated constraint.</param>
    /// <returns>A new constraint violation error.</returns>
    public static PersistenceError ConstraintViolation(string? detail) =>
        new ConstraintViolationError(string.IsNullOrWhiteSpace(detail) ? "A constraint violation occurred: unknown error." : detail);

    /// <summary>Creates a <see cref="PersistenceUnexpectedError"/>.</summary>
    /// <param name="message">Optional message describing the unexpected error.</param>
    /// <returns>A new unexpected persistence error.</returns>
    public static PersistenceError Unexpected(string? message) =>
        new PersistenceUnexpectedError(string.IsNullOrWhiteSpace(message) ? "An unexpected persistence error occurred: unknown error." : message);
}
