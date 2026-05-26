using System.Collections.Generic;
using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Workspace;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit;

public class GivenAWorkspaceViewModel
{
    [Fact]
    public void when_constructed_then_accounts_contains_four_entries()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts.Count.ShouldBe(4);
    }

    [Fact]
    public void when_constructed_then_accounts_are_in_provider_order()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts[0].Kind.ShouldBe(ProviderKind.OneDrive);
        sut.Accounts[1].Kind.ShouldBe(ProviderKind.GoogleDrive);
        sut.Accounts[2].Kind.ShouldBe(ProviderKind.GoogleDrive);
        sut.Accounts[3].Kind.ShouldBe(ProviderKind.Dropbox);
    }

    [Fact]
    public void when_constructed_then_selected_account_is_first_account()
    {
        var sut = new WorkspaceViewModel();

        sut.SelectedAccount.ShouldBeSameAs(sut.Accounts[0]);
    }

    [Fact]
    public void when_constructed_then_first_account_is_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts[0].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void when_constructed_then_non_first_accounts_are_not_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.Accounts[1].IsSelected.ShouldBeFalse();
        sut.Accounts[2].IsSelected.ShouldBeFalse();
        sut.Accounts[3].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_today_buckets_has_twenty_four_entries()
    {
        var sut = new WorkspaceViewModel();

        sut.TodayBuckets.Length.ShouldBe(24);
    }

    [Fact]
    public void when_selected_account_is_set_then_property_changed_fires()
    {
        var sut = new WorkspaceViewModel();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.SelectedAccount = sut.Accounts[1];

        raisedProperties.ShouldContain(nameof(WorkspaceViewModel.SelectedAccount));
    }

    [Fact]
    public void when_selected_account_is_changed_then_new_account_is_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.SelectedAccount = sut.Accounts[2];

        sut.Accounts[2].IsSelected.ShouldBeTrue();
    }

    [Fact]
    public void when_selected_account_is_changed_then_previous_account_is_not_marked_as_selected()
    {
        var sut = new WorkspaceViewModel();

        sut.SelectedAccount = sut.Accounts[2];

        sut.Accounts[0].IsSelected.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_download_rate_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.DownloadRate.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_upload_rate_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.UploadRate.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_queue_summary_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.QueueSummary.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_version_is_not_empty()
    {
        var sut = new WorkspaceViewModel();

        sut.Version.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_constructed_then_workspace_subtitle_contains_account_count()
    {
        var sut = new WorkspaceViewModel();

        sut.WorkspaceSubtitle.ShouldContain("4");
    }

    [Fact]
    public void when_constructed_then_workspace_subtitle_contains_total_storage()
    {
        var sut = new WorkspaceViewModel();

        sut.WorkspaceSubtitle.ShouldContain("TB");
    }
}
