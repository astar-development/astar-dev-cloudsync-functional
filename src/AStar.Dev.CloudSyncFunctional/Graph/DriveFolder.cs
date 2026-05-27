namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>A folder in a OneDrive drive.</summary>
/// <param name="Id">The Graph drive item ID.</param>
/// <param name="Name">The folder display name.</param>
/// <param name="ParentId">The parent folder ID, or null for root-level folders.</param>
public sealed record DriveFolder(string Id, string Name, string? ParentId);
