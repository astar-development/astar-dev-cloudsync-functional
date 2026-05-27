using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AStar.Dev.CloudSyncFunctional.Persistence;

/// <summary>Centralised EF Core value converters for SQLite-incompatible .NET types.</summary>
public static class SqliteTypeConverters
{
    /// <summary>Converts <see cref="AccountId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<AccountId, string> AccountIdConverter { get; } =
        new(id => id.Value, str => new AccountId(str));

    /// <summary>Converts <see cref="DriveId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<DriveId, string> DriveIdConverter { get; } =
        new(id => id.Value, str => new DriveId(str));

    /// <summary>Converts <see cref="OneDriveItemId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<OneDriveItemId, string> OneDriveItemIdConverter { get; } =
        new(id => id.Value, str => new OneDriveItemId(str));

    /// <summary>Converts <see cref="OneDriveFolderId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<OneDriveFolderId, string> OneDriveFolderIdConverter { get; } =
        new(id => id.Value, str => new OneDriveFolderId(str));

    /// <summary>Converts <see cref="SyncRuleId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<SyncRuleId, string> SyncRuleIdConverter { get; } =
        new(id => id.Value, str => new SyncRuleId(str));

    /// <summary>Converts <see cref="SyncedItemId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<SyncedItemId, string> SyncedItemIdConverter { get; } =
        new(id => id.Value, str => new SyncedItemId(str));

    /// <summary>Converts <see cref="SyncJobId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<SyncJobId, string> SyncJobIdConverter { get; } =
        new(id => id.Value, str => new SyncJobId(str));

    /// <summary>Converts <see cref="SyncConflictId"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<SyncConflictId, string> SyncConflictIdConverter { get; } =
        new(id => id.Value, str => new SyncConflictId(str));

    /// <summary>Converts <see cref="EmailAddress"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<EmailAddress, string> EmailAddressConverter { get; } =
        new(e => e.Value, str => new EmailAddress(str));

    /// <summary>Converts <see cref="DisplayName"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<DisplayName, string> DisplayNameConverter { get; } =
        new(d => d.Value, str => new DisplayName(str));

    /// <summary>Converts <see cref="LocalPath"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<LocalPath, string> LocalPathConverter { get; } =
        new(p => p.Value, str => new LocalPath(str));

    /// <summary>Converts <see cref="LocalSyncPath"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<LocalSyncPath, string> LocalSyncPathConverter { get; } =
        new(p => p.Value, str => new LocalSyncPath(str));

    /// <summary>Converts <see cref="RemotePath"/> to and from <see langword="string"/>.</summary>
    public static ValueConverter<RemotePath, string> RemotePathConverter { get; } =
        new(p => p.Value, str => new RemotePath(str));

    /// <summary>Converts a nullable <see cref="DateTimeOffset"/> to and from a nullable UTC ticks <see langword="long"/>.</summary>
    public static ValueConverter<DateTimeOffset?, long?> NullableDateTimeOffsetToTicks { get; } =
        new(dt => dt.HasValue ? dt.Value.UtcTicks : (long?)null,
            ticks => ticks.HasValue ? new DateTimeOffset(ticks.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
}
