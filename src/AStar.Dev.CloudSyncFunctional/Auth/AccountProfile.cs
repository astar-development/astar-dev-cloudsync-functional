namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>Profile information extracted from a successful authentication result.</summary>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
public sealed record AccountProfile(string DisplayName, string Email);

public static class AccountProfileFactory
{
    /// <summary>
    /// Factory method to create an AccountProfile from string values. This can be extended in the future to include validation or transformation logic if needed.
    /// </summary>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="email">The user's email address.</param>
    /// <returns>An <see cref="AccountProfile"/> with the provided values.</returns>
    public static AccountProfile Create(string displayName, string email) => new(displayName, email);
}