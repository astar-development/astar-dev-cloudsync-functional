using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Sync;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Sync;

public class GivenASyncError
{
    [Fact]
    public void when_NoFoldersConfiguredError_created_then_message_is_correct()
    {
        var error = SyncErrorFactory.NoFoldersConfigured();

        error.Message.ShouldBe("No folders have been configured for sync.");
    }

    [Fact]
    public void when_SyncCancelledError_created_then_message_is_correct()
    {
        var error = SyncErrorFactory.Cancelled();

        error.Message.ShouldBe("Sync was cancelled.");
    }

    [Fact]
    public void when_SyncAuthError_wraps_auth_error_then_message_delegates()
    {
        var inner = AuthErrorFactory.Failed("token expired");

        var error = SyncErrorFactory.AuthFailed(inner);

        error.Message.ShouldBe("token expired");
    }

    [Fact]
    public void when_SyncGraphError_wraps_graph_error_then_message_delegates()
    {
        var inner = GraphErrorFactory.Unexpected("graph fail");

        var error = SyncErrorFactory.GraphFailed(inner);

        error.Message.ShouldBe("graph fail");
    }

    [Fact]
    public void when_SyncStorageError_wraps_persistence_error_then_message_delegates()
    {
        var inner = PersistenceErrorFactory.Unexpected("db fail");

        var error = SyncErrorFactory.StorageFailed(inner);

        error.Message.ShouldBe("db fail");
    }
}
