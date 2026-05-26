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

    [Fact]
    public void when_no_children_then_has_children_is_false()
    {
        var sut = new FolderNode();

        sut.HasChildren.ShouldBeFalse();
    }

    [Fact]
    public void when_children_collection_has_items_then_has_children_is_true()
    {
        var sut = new FolderNode();
        sut.Children.Add(new FolderNode { Name = "sub" });

        sut.HasChildren.ShouldBeTrue();
    }
}
