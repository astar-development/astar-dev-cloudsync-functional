namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>Base type for authentication errors.</summary>
public abstract record AuthError
{
    /// <summary>Gets the human-readable error message.</summary>
    public abstract string Message { get; }
}

/// <summary>Represents a cancelled authentication attempt.</summary>
public sealed record AuthCancelledError : AuthError
{
    /// <inheritdoc/>
    public override string Message => "Authentication was cancelled.";
}

/// <summary>Represents a failed authentication attempt with a specific reason.</summary>
public sealed record AuthFailedError : AuthError
{
    /// <inheritdoc/>
    public override string Message { get; }

    /// <summary>Initialises a new <see cref="AuthFailedError"/> with the given message.</summary>
    /// <param name="message">The error message describing the failure.</param>
    public AuthFailedError(string message) => Message = message;
}

/// <summary>Creates <see cref="AuthError"/> instances.</summary>
public static class AuthErrorFactory
{
    /// <summary>Creates an <see cref="AuthCancelledError"/>.</summary>
    /// <returns>An error representing a cancelled authentication.</returns>
    public static AuthError Cancelled() => new AuthCancelledError();

    /// <summary>Creates an <see cref="AuthFailedError"/> with the given message.</summary>
    /// <param name="message">The error message; falls back to a default if null or whitespace.</param>
    /// <returns>An error representing a failed authentication.</returns>
    public static AuthError Failed(string? message) => new AuthFailedError(
        string.IsNullOrWhiteSpace(message) ? "Authentication failed: unknown error." : message);
}
