namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>Base type for Graph API errors.</summary>
public abstract record GraphError
{
    /// <summary>Gets the human-readable error message.</summary>
    public abstract string Message { get; }
}

/// <summary>The requested item was not found in OneDrive.</summary>
public sealed record GraphNotFoundError : GraphError
{
    /// <summary>Gets the ID of the item that was not found.</summary>
    public string ItemId { get; }

    /// <inheritdoc/>
    public override string Message => $"Item '{ItemId}' was not found in OneDrive.";

    /// <summary>Initialises a new <see cref="GraphNotFoundError"/>.</summary>
    /// <param name="itemId">The ID of the item that was not found.</param>
    public GraphNotFoundError(string itemId) => ItemId = itemId;
}

/// <summary>The Graph API request was throttled.</summary>
public sealed record GraphThrottledError : GraphError
{
    /// <summary>Gets how long to wait before retrying, in seconds.</summary>
    public int RetryAfterSeconds { get; }

    /// <inheritdoc/>
    public override string Message => $"Request throttled. Retry after {RetryAfterSeconds} seconds.";

    /// <summary>Initialises a new <see cref="GraphThrottledError"/>.</summary>
    /// <param name="retryAfterSeconds">Seconds to wait before retrying.</param>
    public GraphThrottledError(int retryAfterSeconds) => RetryAfterSeconds = retryAfterSeconds;
}

/// <summary>The request was rejected due to invalid or expired credentials.</summary>
public sealed record GraphUnauthorizedError : GraphError
{
    /// <inheritdoc/>
    public override string Message => "Unauthorized. Re-authentication required.";
}

/// <summary>A network-level error occurred during a Graph API call.</summary>
public sealed record GraphNetworkError : GraphError
{
    /// <inheritdoc/>
    public override string Message { get; }

    /// <summary>Initialises a new <see cref="GraphNetworkError"/>.</summary>
    /// <param name="message">The error message.</param>
    public GraphNetworkError(string message) => Message = message;
}

/// <summary>An unexpected error occurred during a Graph API call.</summary>
public sealed record GraphUnexpectedError : GraphError
{
    /// <inheritdoc/>
    public override string Message { get; }

    /// <summary>Initialises a new <see cref="GraphUnexpectedError"/>.</summary>
    /// <param name="message">The error message.</param>
    public GraphUnexpectedError(string message) => Message = message;
}

/// <summary>Creates <see cref="GraphError"/> instances.</summary>
public static class GraphErrorFactory
{
    /// <summary>Creates a <see cref="GraphNotFoundError"/>.</summary>
    /// <param name="itemId">The ID of the item that was not found.</param>
    /// <returns>A not-found error.</returns>
    public static GraphError NotFound(string itemId) => new GraphNotFoundError(itemId);

    /// <summary>Creates a <see cref="GraphThrottledError"/>.</summary>
    /// <param name="retryAfterSeconds">Seconds to wait before retrying.</param>
    /// <returns>A throttled error.</returns>
    public static GraphError Throttled(int retryAfterSeconds) => new GraphThrottledError(retryAfterSeconds);

    /// <summary>Creates a <see cref="GraphUnauthorizedError"/>.</summary>
    /// <returns>An unauthorized error.</returns>
    public static GraphError Unauthorized() => new GraphUnauthorizedError();

    /// <summary>Creates a <see cref="GraphNetworkError"/>.</summary>
    /// <param name="message">The error message; falls back to a default if null or whitespace.</param>
    /// <returns>A network error.</returns>
    public static GraphError Network(string? message) => new GraphNetworkError(
        string.IsNullOrWhiteSpace(message) ? "A network error occurred: unknown error." : message);

    /// <summary>Creates a <see cref="GraphUnexpectedError"/>.</summary>
    /// <param name="message">The error message; falls back to a default if null or whitespace.</param>
    /// <returns>An unexpected error.</returns>
    public static GraphError Unexpected(string? message) => new GraphUnexpectedError(
        string.IsNullOrWhiteSpace(message) ? "An unexpected Graph error occurred: unknown error." : message);
}
