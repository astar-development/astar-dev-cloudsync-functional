using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAnOption
{
    [Fact]
    public void when_a_some_option_is_created_then_implicit_conversion_returns_the_value()
    {
        Option<int> result = new Some<int>(42);

        int value = result;

        value.ShouldBe(42);
    }

    [Fact]
    public void when_a_none_option_is_created_then_implicit_conversion_returns_the_error()
    {
        Option<int> result = new None<int>();

        string error = result;

        error.ShouldBe("missing");
    }
}
