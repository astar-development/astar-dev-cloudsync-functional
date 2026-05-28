namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Creates <see cref="ISyncWorker"/> instances for parallel pipeline execution.</summary>
public interface ISyncWorkerFactory
{
    /// <summary>Creates a new <see cref="ISyncWorker"/> for executing sync jobs.</summary>
    /// <returns>A new worker instance.</returns>
    ISyncWorker Create();
}
