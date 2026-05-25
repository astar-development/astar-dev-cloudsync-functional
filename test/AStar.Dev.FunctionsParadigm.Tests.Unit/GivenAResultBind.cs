using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAResultBind
{
    [Fact]
    public void when_binding_an_ok_result_then_the_bound_result_is_returned()
    {
        Result<int, string> result = new Ok<int, string>(7);

        Result<int, string> bound = result.Bind(value => new Ok<int, string>(value + 5));

        Assert.Equal(12, bound.Match(value => value, _ => -1));
    }

    [Fact]
    public void when_binding_a_fail_result_then_the_failure_is_preserved()
    {
        Result<int, string> result = new Fail<int, string>("nope");

        Result<int, string> bound = result.Bind(value => new Ok<int, string>(value + 1));

        Assert.Equal("nope", bound.Match(_ => string.Empty, error => error));
    }
}