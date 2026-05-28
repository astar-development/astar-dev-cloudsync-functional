using System.Net;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Graph;

public sealed class GivenAGraphService
{
    private static GraphService CreateSut(IGraphClientFactory? factory = null) =>
        new(factory ?? Substitute.For<IGraphClientFactory>(), Substitute.For<ILogger<GraphService>>());

    [Fact]
    public async Task when_client_factory_throws_then_get_root_folders_returns_unexpected_graph_error()
    {
        var factory = Substitute.For<IGraphClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => throw new Exception("network failure"));
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
        factory.CreateClient(Arg.Any<string>()).Returns(_ => throw new Exception("network failure"));
        var sut = CreateSut(factory);

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task when_drive_is_null_then_get_root_folders_returns_unexpected_graph_error()
    {
        var sut = CreateSut(CreateFactory(
            NoContentResponse()));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("Drive was null.");
    }

    [Fact]
    public async Task when_root_is_null_then_get_root_folders_returns_unexpected_graph_error()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            NoContentResponse()));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("Root item was null.");
    }

    [Fact]
    public async Task when_root_id_is_null_then_get_root_folders_returns_unexpected_graph_error()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "name": "root" }""")));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("Root item was null.");
    }

    [Fact]
    public async Task when_first_folder_page_is_null_then_get_root_folders_returns_unexpected_graph_error()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            NoContentResponse()));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("Folder page was null.");
    }

    [Fact]
    public async Task when_root_has_no_children_then_get_root_folders_returns_empty_success()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            JsonResponse("""{ "value": [] }""")));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Ok<List<DriveFolder>, GraphError>>();
        ok.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_root_contains_files_and_folders_then_get_root_folders_returns_only_folders()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            JsonResponse(
                """
                {
                  "value": [
                    {
                      "id": "folder-1",
                      "name": "Documents",
                      "folder": {},
                      "parentReference": { "id": "root-id" }
                    },
                    {
                      "id": "file-1",
                      "name": "notes.txt",
                      "file": {}
                    },
                    {
                      "id": "folder-2",
                      "name": "Pictures",
                      "folder": {}
                    }
                  ]
                }
                """)));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Ok<List<DriveFolder>, GraphError>>();
        ok.Value.Count.ShouldBe(2);
        ok.Value[0].ShouldBe(new DriveFolder("folder-1", "Documents", "root-id"));
        ok.Value[1].ShouldBe(new DriveFolder("folder-2", "Pictures", null));
    }

    [Fact]
    public async Task when_folder_results_are_paged_then_get_root_folders_returns_folders_from_all_pages()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            JsonResponse(
                """
                {
                  "@odata.nextLink": "https://graph.microsoft.com/v1.0/drives/drive-id/items/root-id/children?$skiptoken=next",
                  "value": [
                    {
                      "id": "folder-1",
                      "name": "Documents",
                      "folder": {},
                      "parentReference": { "id": "root-id" }
                    }
                  ]
                }
                """),
            JsonResponse(
                """
                {
                  "value": [
                    {
                      "id": "folder-2",
                      "name": "Pictures",
                      "folder": {},
                      "parentReference": { "id": "root-id" }
                    }
                  ]
                }
                """)));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Ok<List<DriveFolder>, GraphError>>();
        ok.Value.ShouldBe([
            new DriveFolder("folder-1", "Documents", "root-id"),
            new DriveFolder("folder-2", "Pictures", "root-id")
        ]);
    }

    [Fact]
    public async Task when_next_folder_page_is_null_then_get_root_folders_returns_unexpected_graph_error()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            JsonResponse(
                """
                {
                  "@odata.nextLink": "https://graph.microsoft.com/v1.0/drives/drive-id/items/root-id/children?$skiptoken=next",
                  "value": [
                    {
                      "id": "folder-1",
                      "name": "Documents",
                      "folder": {}
                    }
                  ]
                }
                """),
            NoContentResponse()));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("Folder page was null.");
    }

    [Fact]
    public async Task when_graph_request_throws_then_get_root_folders_returns_unexpected_graph_error()
    {
        var sut = CreateSut(CreateFactory(
            _ => throw new HttpRequestException("Graph request failed")));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("Graph request failed");
    }
    
    // ... existing code ...

    [Fact]
    public async Task when_client_factory_returns_failure_then_get_root_folders_returns_that_graph_error()
    {
        var factory = Substitute.For<IGraphClientFactory>();
        factory.CreateClient(Arg.Any<string>())
            .Returns(new Fail<GraphServiceClient, GraphError>(GraphErrorFactory.Unexpected("client creation failed")));
        var sut = CreateSut(factory);

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<List<DriveFolder>, GraphError>>();
        fail.Error.ShouldBeOfType<GraphUnexpectedError>();
        fail.Error.Message.ShouldContain("client creation failed");
    }

    [Fact]
    public async Task when_folder_page_has_null_value_then_get_root_folders_returns_empty_success()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            JsonResponse("""{}""")));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Ok<List<DriveFolder>, GraphError>>();
        ok.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_folder_items_have_missing_id_or_name_then_get_root_folders_skips_them()
    {
        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            JsonResponse(
                """
                {
                  "value": [
                    {
                      "name": "Missing Id",
                      "folder": {},
                      "parentReference": { "id": "root-id" }
                    },
                    {
                      "id": "missing-name",
                      "folder": {},
                      "parentReference": { "id": "root-id" }
                    },
                    {
                      "id": "valid-folder",
                      "name": "Valid Folder",
                      "folder": {},
                      "parentReference": { "id": "root-id" }
                    }
                  ]
                }
                """)));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Ok<List<DriveFolder>, GraphError>>();
        ok.Value.ShouldBe([
            new DriveFolder("valid-folder", "Valid Folder", "root-id")
        ]);
    }

    [Fact]
    public async Task when_requesting_first_folder_page_then_expected_select_fields_are_requested()
    {
        HttpRequestMessage? childrenRequest = null;

        var sut = CreateSut(CreateFactory(
            JsonResponse("""{ "id": "drive-id" }"""),
            JsonResponse("""{ "id": "root-id", "name": "root" }"""),
            request =>
            {
                childrenRequest = request;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{ "value": [] }""", System.Text.Encoding.UTF8, "application/json")
                };
            }));

        var result = await sut.GetRootFoldersAsync("account-id", "access-token", TestContext.Current.CancellationToken);

        var ok = result.ShouldBeOfType<Ok<List<DriveFolder>, GraphError>>();
        ok.Value.ShouldBeEmpty();

        childrenRequest.ShouldNotBeNull();
        var decodedQuery = Uri.UnescapeDataString(childrenRequest.RequestUri?.Query ?? string.Empty);

        decodedQuery.ShouldContain("$select=");
        decodedQuery.ShouldContain("id");
        decodedQuery.ShouldContain("name");
        decodedQuery.ShouldContain("folder");
        decodedQuery.ShouldContain("parentReference");
    }

    private static IGraphClientFactory CreateFactory(params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
    {
        var factory = Substitute.For<IGraphClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new Ok<GraphServiceClient, GraphError>(new GraphServiceClient(
            new HttpClient(new StubGraphMessageHandler(responses))
            {
                BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
            },
            new AnonymousAuthenticationProvider())));

        return factory;
    }

    private static Func<HttpRequestMessage, HttpResponseMessage> JsonResponse(string json) =>
        _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

    private static Func<HttpRequestMessage, HttpResponseMessage> NoContentResponse() =>
        _ => new HttpResponseMessage(HttpStatusCode.NoContent);

    private sealed class StubGraphMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses) : HttpMessageHandler
    {
        private int requestIndex;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requestIndex.ShouldBeLessThan(
                responses.Length,
                $"Unexpected Graph request: {request.Method} {request.RequestUri}");

            return Task.FromResult(responses[requestIndex++](request));
        }
    }
}