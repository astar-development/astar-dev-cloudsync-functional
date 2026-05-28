using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;

namespace AStar.Dev.CloudSyncFunctional.Recovery;

/// <summary>Describes an interrupted sync detected on startup for a specific account.</summary>
/// <param name="AccountId">The account identifier.</param>
/// <param name="AccountName">The human-readable account display name.</param>
/// <param name="CanResume">True when a delta token exists and sync can resume from the interruption point.</param>
/// <param name="Message">A user-facing explanation of the recovery status.</param>
public sealed record InterruptedSyncInfo(AccountId AccountId, string AccountName, bool CanResume, string Message);
