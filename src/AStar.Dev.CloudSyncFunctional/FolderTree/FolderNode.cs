using System;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace AStar.Dev.CloudSyncFunctional.FolderTree;

/// <summary>Represents a single folder in the cloud provider's folder tree.</summary>
public class FolderNode : ReactiveObject
{
    /// <summary>Gets or sets the absolute path of this folder.</summary>
    public string Path { get; set => this.RaiseAndSetIfChanged(ref field, value); } = string.Empty;

    /// <summary>Gets or sets the display name of this folder.</summary>
    public string Name { get; set => this.RaiseAndSetIfChanged(ref field, value); } = string.Empty;

    /// <summary>Gets or sets the nesting depth relative to the account root.</summary>
    public int Depth { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets the number of immediate child folders.</summary>
    public int ChildCount { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets the total size of the folder in bytes.</summary>
    public long SizeBytes { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets the time this folder was last synchronised.</summary>
    public DateTimeOffset LastSync { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets the tri-state selection for sync inclusion.</summary>
    public CheckState SelectionState { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets whether this node is expanded in the tree view.</summary>
    public bool IsExpanded { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets whether this folder is currently being synchronised.</summary>
    public bool IsSyncing { get; set => this.RaiseAndSetIfChanged(ref field, value); }

    /// <summary>Gets or sets the immediate child folder nodes.</summary>
    public ObservableCollection<FolderNode> Children { get; set => this.RaiseAndSetIfChanged(ref field, value); } = [];
}
