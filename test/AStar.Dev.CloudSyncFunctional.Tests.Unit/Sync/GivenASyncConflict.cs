using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Sync;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Sync;

public class GivenASyncConflict
{
    [Fact]
    public void when_sync_conflict_created_then_state_is_pending()
    {
        var conflict = new SyncConflict(new SyncConflictId("c1"), "acc1", "item1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, ConflictState.Pending);

        conflict.State.ShouldBe(ConflictState.Pending);
    }

    [Fact]
    public void when_sync_conflict_state_is_pending_then_is_not_resolved()
    {
        var conflict = new SyncConflict(new SyncConflictId("c1"), "acc1", "item1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, ConflictState.Pending);

        conflict.State.ShouldNotBe(ConflictState.Resolved);
    }
}
