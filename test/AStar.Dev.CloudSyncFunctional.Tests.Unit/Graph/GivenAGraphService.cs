using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Graph;

public sealed class GivenAGraphService
{
    private static GraphService CreateSut(IGraphClientFactory? factory = null) =>
        new(factory ?? Substitute.For<IGraphClientFactory>(), Substitute.For<ILogger<GraphService>>());

    [Fact]
    public async Task when_client_factory_throws_then_get_root_folders_returns_unexpected_graph_error()
    {
        var factory = Substitute.For<IGraphClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns<GraphServiceClient>(_ => throw new Exception("network failure"));
        var sut = CreateSut(factory);

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("network failure");
    }

    [Fact]
    public async Task when_client_factory_throws_then_error_message_is_not_null_or_empty()
    {
        var factory = Substitute.For<IGraphClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns<GraphServiceClient>(_ => throw new Exception("network failure"));
        var sut = CreateSut(factory);

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.Message.ShouldNotBeNullOrEmpty();
    }
}
