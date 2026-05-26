using System.Collections.Generic;
using AStar.Dev.CloudSyncFunctional.ViewModels;

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
}
