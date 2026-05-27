using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Persistence.Entities;

/// <summary>Profile information stored as a complex property on <see cref="AccountEntity"/>.</summary>
public sealed class AccountProfileEntity
{
    /// <summary>Gets or sets the display name.</summary>
    public DisplayName DisplayName { get; set; }

    /// <summary>Gets or sets the email address.</summary>
    public EmailAddress Email { get; set; }
}
