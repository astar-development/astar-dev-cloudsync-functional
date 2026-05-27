namespace AStar.Dev.CloudSyncFunctional.Domain;

/// <summary>A OneDrive root folder selected for sync, carrying both the Graph item ID and the display name.</summary>
/// <param name="Id">The Graph drive item identifier.</param>
/// <param name="Name">The folder display name (e.g. "Documents").</param>
public readonly record struct SelectedFolder(string Id, string Name);
