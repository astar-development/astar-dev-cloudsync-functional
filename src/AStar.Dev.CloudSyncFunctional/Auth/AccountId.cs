namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>
/// Strongly-typed wrapper for an OneDrive account identifier.
/// </summary>
/// <param name="Value">The account identifier.</param>
public sealed record AccountId(string Value)
{
    /// <summary>
    /// Factory method to create an AccountId from a string value. This can be extended in the future to include validation or transformation logic if needed.
    /// </summary>
    /// <param name="value">The string value to create the AccountId from.</param>
    /// <returns>The created AccountId.</returns>
    public static AccountId Create(string value) => new(value);
}
