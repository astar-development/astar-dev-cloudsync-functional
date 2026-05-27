using AStar.Dev.CloudSyncFunctional.Graph;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Graph;

public class GivenAGraphError
{
    [Fact]
    public void when_not_found_is_called_then_result_is_graph_not_found_error()
    {
        var error = GraphErrorFactory.NotFound("item-123");

        error.ShouldBeOfType<GraphNotFoundError>();
    }

    [Fact]
    public void when_not_found_is_called_then_item_id_appears_in_message()
    {
        var error = GraphErrorFactory.NotFound("item-123");

        error.Message.ShouldContain("item-123");
    }

    [Fact]
    public void when_network_is_called_with_null_message_then_default_message_is_used()
    {
        var error = GraphErrorFactory.Network(null);

        error.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_network_is_called_with_message_then_message_is_preserved()
    {
        var error = GraphErrorFactory.Network("connection refused");

        error.Message.ShouldBe("connection refused");
    }

    [Fact]
    public void when_unexpected_is_called_with_null_then_default_message_is_used()
    {
        var error = GraphErrorFactory.Unexpected(null);

        error.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_throttled_is_called_then_retry_seconds_appear_in_message()
    {
        var error = GraphErrorFactory.Throttled(30);

        error.Message.ShouldContain("30");
    }

    [Fact]
    public void when_unauthorized_is_called_then_message_is_not_empty()
    {
        var error = GraphErrorFactory.Unauthorized();

        error.Message.ShouldNotBeNullOrEmpty();
    }
}
