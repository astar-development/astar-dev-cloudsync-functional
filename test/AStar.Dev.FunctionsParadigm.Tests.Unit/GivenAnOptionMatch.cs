using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAnOptionMatch
{
    [Fact]
    public void when_matching_a_some_option_then_the_success_handler_is_used()
    {
        Option<int> result = new Some<int>(8);

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        matched.ShouldBe("ok:8");
    }

    [Fact]
    public void when_matching_a_none_option_then_the_failure_handler_is_used()
    {
        Option<int> result = new None<int>();

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        matched.ShouldBe("fail:missing");
    }
}
