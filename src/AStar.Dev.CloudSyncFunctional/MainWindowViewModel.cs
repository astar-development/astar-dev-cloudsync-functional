using ReactiveUI;
using AStar.Dev.CloudSyncFunctional.ViewModels;

namespace AStar.Dev.CloudSyncFunctional;

/// <summary>Top-level view-model that owns the workspace and is bound to the main window.</summary>
public class MainWindowViewModel : ReactiveObject
{
    /// <summary>Gets the workspace containing all accounts and summary data.</summary>
    public WorkspaceViewModel Workspace { get; } = new WorkspaceViewModel();
}
