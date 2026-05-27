namespace AStar.Dev.CloudSyncFunctional.Wizard;

/// <summary>Identifies the current step in the add-account wizard.</summary>
public enum WizardStep
{
    /// <summary>The user selects a cloud storage provider.</summary>
    ProviderSelection,

    /// <summary>The user signs in with their chosen provider.</summary>
    SignIn,

    /// <summary>The user selects which folders to sync.</summary>
    SelectFolders
}
