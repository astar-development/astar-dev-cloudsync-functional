using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAnOptionBind
{
    [Fact]
    public void when_binding_a_some_option_then_the_bound_result_is_returned()
    {
        Option<int> result = new Some<int>(7);

        var bound = result.Bind(value => new Some<int>(value + 5));

        bound.Match(value => value, _ => -1).ShouldBe(12);
    }

    [Fact]
    public void when_binding_a_none_option_then_the_failure_is_preserved()
    {
        Option<int> result = new None<int>();

        var bound = result.Bind(value => new None<int>());

        bound.Match(_ => string.Empty, error => error).ShouldBe("missing");
    }
}
