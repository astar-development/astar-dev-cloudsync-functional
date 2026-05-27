namespace AStar.Dev.CloudSyncFunctional.Auth;

/// <summary>Profile information extracted from a successful authentication result.</summary>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Email">The user's email address.</param>
public sealed record AccountProfile(string DisplayName, string Email);
