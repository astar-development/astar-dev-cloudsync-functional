using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public class GivenAProviderMark
{
    [Fact]
    public void when_kind_is_onedrive_then_letter_is_O()
    {
        var (letter, _, _) = ProviderMark.GetProviderInfo(ProviderKind.OneDrive);

        letter.ShouldBe("O");
    }

    [Fact]
    public void when_kind_is_googledrive_then_letter_is_G()
    {
        var (letter, _, _) = ProviderMark.GetProviderInfo(ProviderKind.GoogleDrive);

        letter.ShouldBe("G");
    }

    [Fact]
    public void when_kind_is_dropbox_then_letter_is_D()
    {
        var (letter, _, _) = ProviderMark.GetProviderInfo(ProviderKind.Dropbox);

        letter.ShouldBe("D");
    }

    [Fact]
    public void when_kind_is_onedrive_then_bg_key_is_OneDriveAccentWeak()
    {
        var (_, bgKey, _) = ProviderMark.GetProviderInfo(ProviderKind.OneDrive);

        bgKey.ShouldBe("OneDriveAccentWeak");
    }

    [Fact]
    public void when_kind_is_googledrive_then_bg_key_is_GoogleDriveAccentWeak()
    {
        var (_, bgKey, _) = ProviderMark.GetProviderInfo(ProviderKind.GoogleDrive);

        bgKey.ShouldBe("GoogleDriveAccentWeak");
    }

    [Fact]
    public void when_kind_is_dropbox_then_bg_key_is_DropboxAccentWeak()
    {
        var (_, bgKey, _) = ProviderMark.GetProviderInfo(ProviderKind.Dropbox);

        bgKey.ShouldBe("DropboxAccentWeak");
    }
}
