using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Graph;
using LocalGraphClientFactory = AStar.Dev.CloudSyncFunctional.Graph.GraphClientFactory;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Graph;

public sealed class GivenALocalGraphClientFactory
{
    [Fact]
    public void when_create_client_with_valid_token_then_returns_non_null_client()
    {
        var result = new LocalGraphClientFactory().CreateClient("test-token");

        var ok = result.ShouldBeOfType<Ok<GraphServiceClient, GraphError>>();
        ok.Value.ShouldNotBeNull();
    }

    [Fact]
    public void when_create_client_with_different_tokens_then_returns_distinct_instances()
    {
        var factory = new LocalGraphClientFactory();

        var ok1 = factory.CreateClient("token-a").ShouldBeOfType<Ok<GraphServiceClient, GraphError>>();
        var ok2 = factory.CreateClient("token-b").ShouldBeOfType<Ok<GraphServiceClient, GraphError>>();

        ok1.Value.ShouldNotBeSameAs(ok2.Value);
    }

    [Fact]
    public void when_create_client_with_null_token_then_returns_failure()
    {
        var result = new LocalGraphClientFactory().CreateClient(null!);

        result.ShouldBeOfType<Fail<GraphServiceClient, GraphError>>();
    }

    [Fact]
    public void when_create_client_with_empty_token_then_returns_failure()
    {
        var result = new LocalGraphClientFactory().CreateClient(string.Empty);

        result.ShouldBeOfType<Fail<GraphServiceClient, GraphError>>();
    }
}
