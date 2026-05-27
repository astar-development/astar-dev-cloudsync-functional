using ReactiveUI;

namespace AStar.Dev.CloudSyncFunctional.Wizard;

/// <summary>Represents a OneDrive root folder shown in the wizard's folder-selection step.</summary>
public sealed class WizardFolderItem : ReactiveObject
{
    /// <summary>Gets the Graph drive item ID.</summary>
    public string FolderId { get; init; } = string.Empty;

    /// <summary>Gets the folder display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets whether this folder is selected for sync.</summary>
    public bool IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}
