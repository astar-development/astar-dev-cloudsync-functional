using System.Collections.Generic;
using AStar.Dev.CloudSyncFunctional.Accounts;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit;

public class GivenAnAccountViewModel
{
    [Fact]
    public void when_status_is_set_then_property_changed_fires()
    {
        var sut = new AccountViewModel { Kind = ProviderKind.OneDrive, Name = "Test", Email = "t@t.com" };
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.Status = SyncStatus.Syncing;

        raisedProperties.ShouldContain(nameof(AccountViewModel.Status));
    }

    [Fact]
    public void when_status_is_set_to_same_value_then_property_changed_does_not_fire()
    {
        var sut = new AccountViewModel { Kind = ProviderKind.OneDrive, Name = "Test", Email = "t@t.com" };
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.Status = SyncStatus.Ok;

        raisedProperties.ShouldBeEmpty();
    }

    [Fact]
    public void when_is_selected_is_set_then_property_changed_fires()
    {
        var sut = new AccountViewModel { Kind = ProviderKind.OneDrive, Name = "Test", Email = "t@t.com" };
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.IsSelected = true;

        raisedProperties.ShouldContain(nameof(AccountViewModel.IsSelected));
    }

    [Fact]
    public void when_is_selected_is_set_to_same_value_then_property_changed_does_not_fire()
    {
        var sut = new AccountViewModel { Kind = ProviderKind.OneDrive, Name = "Test", Email = "t@t.com" };
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.IsSelected = false;

        raisedProperties.ShouldBeEmpty();
    }
}
