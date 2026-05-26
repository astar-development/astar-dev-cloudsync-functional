using System.Collections.Generic;
using AStar.Dev.CloudSyncFunctional.ViewModels;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit;

public class GivenAFolderNode
{
    [Fact]
    public void when_name_is_set_then_property_changed_fires()
    {
        var sut = new FolderNode();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.Name = "Documents";

        Assert.Contains(nameof(FolderNode.Name), raisedProperties);
    }

    [Fact]
    public void when_is_syncing_is_set_then_property_changed_fires()
    {
        var sut = new FolderNode();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.IsSyncing = true;

        Assert.Contains(nameof(FolderNode.IsSyncing), raisedProperties);
    }
}
