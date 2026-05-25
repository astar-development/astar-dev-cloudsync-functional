using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAResult
{
    [Fact]
    public void when_an_ok_result_is_created_then_implicit_conversion_returns_the_value()
    {
        Result<int, string> result = new Ok<int, string>(42);

        int value = result;

        Assert.Equal(42, value);
    }

    [Fact]
    public void when_a_fail_result_is_created_then_implicit_conversion_returns_the_error()
    {
        Result<int, string> result = new Fail<int, string>("boom");

        string error = result;

        Assert.Equal("boom", error);
    }
}
