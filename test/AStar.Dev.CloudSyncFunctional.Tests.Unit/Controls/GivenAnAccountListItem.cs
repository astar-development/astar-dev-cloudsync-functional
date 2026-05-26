using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public class GivenAnAccountListItem
{
    [Fact]
    public void when_zero_bytes_then_format_gb_returns_zero_point_zero() =>
        AccountListItem.FormatGb(0).ShouldBe("0.0");

    [Fact]
    public void when_one_gigabyte_then_format_gb_returns_one_point_zero() =>
        AccountListItem.FormatGb(1_073_741_824L).ShouldBe("1.0");

    [Fact]
    public void when_two_gigabytes_then_format_gb_returns_two_point_zero() =>
        AccountListItem.FormatGb(2_147_483_648L).ShouldBe("2.0");

    [Fact]
    public void when_half_a_gigabyte_then_format_gb_returns_zero_point_five() =>
        AccountListItem.FormatGb(536_870_912L).ShouldBe("0.5");

    [Fact]
    public void when_one_point_two_gigabytes_then_format_gb_returns_one_point_two() =>
        AccountListItem.FormatGb(1_288_490_188L).ShouldBe("1.2");

    [Fact]
    public void when_status_is_syncing_then_get_status_dot_config_color_key_is_primary()
    {
        var (colorKey, _) = AccountListItem.GetStatusDotConfig(SyncStatus.Syncing);

        colorKey.ShouldBe("Primary");
    }

    [Fact]
    public void when_status_is_syncing_then_get_status_dot_config_is_pulsing()
    {
        var (_, isPulsing) = AccountListItem.GetStatusDotConfig(SyncStatus.Syncing);

        isPulsing.ShouldBeTrue();
    }

    [Fact]
    public void when_status_is_ok_then_get_status_dot_config_color_key_is_good()
    {
        var (colorKey, _) = AccountListItem.GetStatusDotConfig(SyncStatus.Ok);

        colorKey.ShouldBe("Good");
    }

    [Fact]
    public void when_status_is_ok_then_get_status_dot_config_is_not_pulsing()
    {
        var (_, isPulsing) = AccountListItem.GetStatusDotConfig(SyncStatus.Ok);

        isPulsing.ShouldBeFalse();
    }

    [Fact]
    public void when_status_is_warn_then_get_status_dot_config_color_key_is_warn()
    {
        var (colorKey, _) = AccountListItem.GetStatusDotConfig(SyncStatus.Warn);

        colorKey.ShouldBe("Warn");
    }

    [Fact]
    public void when_status_is_warn_then_get_status_dot_config_is_not_pulsing()
    {
        var (_, isPulsing) = AccountListItem.GetStatusDotConfig(SyncStatus.Warn);

        isPulsing.ShouldBeFalse();
    }

    [Fact]
    public void when_status_is_paused_then_get_status_dot_config_color_key_is_ink3()
    {
        var (colorKey, _) = AccountListItem.GetStatusDotConfig(SyncStatus.Paused);

        colorKey.ShouldBe("Ink3");
    }

    [Fact]
    public void when_status_is_paused_then_get_status_dot_config_is_not_pulsing()
    {
        var (_, isPulsing) = AccountListItem.GetStatusDotConfig(SyncStatus.Paused);

        isPulsing.ShouldBeFalse();
    }

    [Fact]
    public void when_kind_is_onedrive_then_get_bar_color_key_returns_onedrive_accent() =>
        AccountListItem.GetBarColorKey(ProviderKind.OneDrive).ShouldBe("OneDriveAccent");

    [Fact]
    public void when_kind_is_googledrive_then_get_bar_color_key_returns_googledrive_accent() =>
        AccountListItem.GetBarColorKey(ProviderKind.GoogleDrive).ShouldBe("GoogleDriveAccent");

    [Fact]
    public void when_kind_is_dropbox_then_get_bar_color_key_returns_dropbox_accent() =>
        AccountListItem.GetBarColorKey(ProviderKind.Dropbox).ShouldBe("DropboxAccent");
}
