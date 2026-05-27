using AStar.Dev.CloudSyncFunctional.Graph;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Graph;

public sealed class GivenAGraphClientFactory
{
    [Fact]
    public void when_create_client_with_valid_token_then_returns_non_null_client() =>
        new GraphClientFactory().CreateClient("test-token").ShouldNotBeNull();

    [Fact]
    public void when_create_client_with_different_tokens_then_returns_distinct_instances()
    {
        var factory = new GraphClientFactory();

        var client1 = factory.CreateClient("token-a");
        var client2 = factory.CreateClient("token-b");

        client1.ShouldNotBeSameAs(client2);
    }
}
