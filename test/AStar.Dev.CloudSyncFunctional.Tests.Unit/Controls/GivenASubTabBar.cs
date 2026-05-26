using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public class GivenASubTabBar
{
    [Fact]
    public void when_tab_is_sync_folders_then_get_tab_label_returns_sync_folders() =>
        SubTabBar.GetTabLabel(SubTab.SyncFolders).ShouldBe("Sync folders");

    [Fact]
    public void when_tab_is_activity_then_get_tab_label_returns_activity() =>
        SubTabBar.GetTabLabel(SubTab.Activity).ShouldBe("Activity");

    [Fact]
    public void when_tab_is_conflicts_then_get_tab_label_returns_conflicts() =>
        SubTabBar.GetTabLabel(SubTab.Conflicts).ShouldBe("Conflicts");

    [Fact]
    public void when_tab_is_settings_then_get_tab_label_returns_settings() =>
        SubTabBar.GetTabLabel(SubTab.Settings).ShouldBe("Settings");
}
