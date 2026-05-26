using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public class GivenAnAccountHeader
{
    [Fact]
    public void when_status_is_ok_then_get_status_pill_config_label_is_all_synced()
    {
        var (label, _) = AccountHeader.GetStatusPillConfig(SyncStatus.Ok);

        label.ShouldBe("All synced");
    }

    [Fact]
    public void when_status_is_ok_then_get_status_pill_config_tone_is_good()
    {
        var (_, tone) = AccountHeader.GetStatusPillConfig(SyncStatus.Ok);

        tone.ShouldBe(Tone.Good);
    }

    [Fact]
    public void when_status_is_syncing_then_get_status_pill_config_label_is_syncing()
    {
        var (label, _) = AccountHeader.GetStatusPillConfig(SyncStatus.Syncing);

        label.ShouldBe("Syncing");
    }

    [Fact]
    public void when_status_is_syncing_then_get_status_pill_config_tone_is_primary()
    {
        var (_, tone) = AccountHeader.GetStatusPillConfig(SyncStatus.Syncing);

        tone.ShouldBe(Tone.Primary);
    }

    [Fact]
    public void when_status_is_warn_then_get_status_pill_config_label_is_warning()
    {
        var (label, _) = AccountHeader.GetStatusPillConfig(SyncStatus.Warn);

        label.ShouldBe("Warning");
    }

    [Fact]
    public void when_status_is_warn_then_get_status_pill_config_tone_is_warn()
    {
        var (_, tone) = AccountHeader.GetStatusPillConfig(SyncStatus.Warn);

        tone.ShouldBe(Tone.Warn);
    }

    [Fact]
    public void when_status_is_paused_then_get_status_pill_config_label_is_paused()
    {
        var (label, _) = AccountHeader.GetStatusPillConfig(SyncStatus.Paused);

        label.ShouldBe("Paused");
    }

    [Fact]
    public void when_status_is_paused_then_get_status_pill_config_tone_is_neutral()
    {
        var (_, tone) = AccountHeader.GetStatusPillConfig(SyncStatus.Paused);

        tone.ShouldBe(Tone.Neutral);
    }

    [Fact]
    public void when_kind_is_onedrive_then_get_provider_display_name_returns_onedrive() =>
        AccountHeader.GetProviderDisplayName(ProviderKind.OneDrive).ShouldBe("OneDrive");

    [Fact]
    public void when_kind_is_googledrive_then_get_provider_display_name_returns_google_drive() =>
        AccountHeader.GetProviderDisplayName(ProviderKind.GoogleDrive).ShouldBe("Google Drive");

    [Fact]
    public void when_kind_is_dropbox_then_get_provider_display_name_returns_dropbox() =>
        AccountHeader.GetProviderDisplayName(ProviderKind.Dropbox).ShouldBe("Dropbox");
}
