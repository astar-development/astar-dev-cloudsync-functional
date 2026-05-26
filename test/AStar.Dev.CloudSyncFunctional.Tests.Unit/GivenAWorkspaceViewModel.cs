using System.Collections.Generic;
using AStar.Dev.CloudSyncFunctional.ViewModels;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit;

public class GivenAWorkspaceViewModel
{
    [Fact]
    public void when_constructed_then_accounts_contains_four_entries()
    {
        var sut = new WorkspaceViewModel();

        Assert.Equal(4, sut.Accounts.Count);
    }

    [Fact]
    public void when_constructed_then_accounts_are_in_provider_order()
    {
        var sut = new WorkspaceViewModel();

        Assert.Equal(ProviderKind.OneDrive,    sut.Accounts[0].Kind);
        Assert.Equal(ProviderKind.GoogleDrive, sut.Accounts[1].Kind);
        Assert.Equal(ProviderKind.GoogleDrive, sut.Accounts[2].Kind);
        Assert.Equal(ProviderKind.Dropbox,     sut.Accounts[3].Kind);
    }

    [Fact]
    public void when_constructed_then_selected_account_is_first_account()
    {
        var sut = new WorkspaceViewModel();

        Assert.Same(sut.Accounts[0], sut.SelectedAccount);
    }

    [Fact]
    public void when_constructed_then_today_buckets_has_twenty_four_entries()
    {
        var sut = new WorkspaceViewModel();

        Assert.Equal(24, sut.TodayBuckets.Length);
    }

    [Fact]
    public void when_selected_account_is_set_then_property_changed_fires()
    {
        var sut = new WorkspaceViewModel();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.SelectedAccount = sut.Accounts[1];

        Assert.Contains(nameof(WorkspaceViewModel.SelectedAccount), raisedProperties);
    }
}
