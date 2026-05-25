using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAnOptionBind
{
    [Fact]
    public void when_binding_a_some_option_then_the_bound_result_is_returned()
    {
        Option<int, string> result = new Some<int, string>(7);

        var bound = result.Bind(value => new Some<int, string>(value + 5));

        Assert.Equal(12, bound.Match(value => value, _ => -1));
    }

    [Fact]
    public void when_binding_a_none_option_then_the_failure_is_preserved()
    {
        Option<int, string> result = new None<int, string>("nope");

        var bound = result.Bind(value => new None<int, string>("nope"));

        Assert.Equal("nope", bound.Match(_ => string.Empty, error => error));
    }
}