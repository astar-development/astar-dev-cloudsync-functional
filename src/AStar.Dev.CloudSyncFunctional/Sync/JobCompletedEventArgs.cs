namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Carries completion information for a single sync job.</summary>
/// <param name="AccountId">The account identifier the job ran for.</param>
/// <param name="RemotePath">The remote OneDrive path the job operated on.</param>
/// <param name="Success">Whether the job completed successfully.</param>
/// <param name="ErrorMessage">The error message when the job failed; <c>null</c> on success.</param>
public sealed record JobCompletedEventArgs(string AccountId, string RemotePath, bool Success, string? ErrorMessage);
