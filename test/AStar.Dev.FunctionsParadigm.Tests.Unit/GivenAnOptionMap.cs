using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAnOptionMap
{
    [Fact]
    public void when_mapping_a_some_option_then_the_value_is_transformed()
    {
        Option<int, string> result = new Some<int, string>(7);

        var mapped = result.Map(value => value * 3);

        mapped.Match(value => value, _ => -1).ShouldBe(21);
    }

    [Fact]
    public void when_mapping_a_none_option_then_the_error_is_preserved()
    {
        Option<int, string> result = new None<int, string>("nope");

        var mapped = result.Map(value => value * 3);

        mapped.Match(_ => string.Empty, error => error).ShouldBe("nope");
    }
}
