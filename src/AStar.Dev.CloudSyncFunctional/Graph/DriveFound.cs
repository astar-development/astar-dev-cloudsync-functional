namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>
/// Represents a drive that was found for a given account, containing the Graph drive information.
/// </summary>
/// <param name="Drive">The Graph drive information.</param>
public sealed record DriveFound(Microsoft.Graph.Models.Drive Drive);