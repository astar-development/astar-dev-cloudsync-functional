using Microsoft.Graph.Models;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>
/// Represents a root folder that was found for a given drive, containing the Graph drive item information.
/// </summary>
/// <param name="DriveItem">The Graph drive item information.</param>
public sealed record RootFound(DriveItem DriveItem);