using System.Collections.Generic;
using AStar.Dev.CloudSyncFunctional.FolderTree;

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

        raisedProperties.ShouldContain(nameof(FolderNode.Name));
    }

    [Fact]
    public void when_is_syncing_is_set_then_property_changed_fires()
    {
        var sut = new FolderNode();
        var raisedProperties = new List<string?>();
        sut.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

        sut.IsSyncing = true;

        raisedProperties.ShouldContain(nameof(FolderNode.IsSyncing));
    }
}
