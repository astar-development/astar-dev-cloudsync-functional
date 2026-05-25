using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAOption
{
    [Fact]
    public void when_a_some_option_is_created_then_implicit_conversion_returns_the_value()
    {
        Option<int, string> result = new Some<int, string>(42);

        int value = result;

        Assert.Equal(42, value);
    }

    [Fact]
    public void when_a_none_option_is_created_then_implicit_conversion_returns_the_error()
    {
        Option<int, string> result = new None<int, string>("missing");

        string error = result;

        Assert.Equal("missing", error);
    }
}