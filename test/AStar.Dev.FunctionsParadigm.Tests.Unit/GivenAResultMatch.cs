using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAResultMatch
{
    [Fact]
    public void when_matching_an_ok_result_then_the_success_handler_is_used()
    {
        Result<int, string> result = new Ok<int, string>(8);

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        matched.ShouldBe("ok:8");
    }

    [Fact]
    public void when_matching_a_fail_result_then_the_failure_handler_is_used()
    {
        Result<int, string> result = new Fail<int, string>("bad");

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        matched.ShouldBe("fail:bad");
    }
}
