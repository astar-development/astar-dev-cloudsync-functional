namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>
/// Strongly-typed wrapper for a Graph drive identifier, which is used to uniquely identify a OneDrive drive or folder across API calls and app sessions.
/// </summary>
/// <param name="Value">The drive identifier.</param>
public sealed record DriveId(string Value)
{
    /// <summary>
    /// Creates a new <see cref="DriveId"/> instance with the specified value.
    /// </summary>
    /// <param name="value">The string value to create the DriveId from.</param>
    /// <returns>The created DriveId.</returns>
    public static DriveId Create(string value) => new(value);
}