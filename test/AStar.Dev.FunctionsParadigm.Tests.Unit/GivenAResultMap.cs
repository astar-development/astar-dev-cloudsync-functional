using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAResultMap
{
    [Fact]
    public void when_mapping_an_ok_result_then_the_value_is_transformed()
    {
        Result<int, string> result = new Ok<int, string>(7);

        Result<int, string> mapped = result.Map(value => value * 3);

        mapped.Match(value => value, _ => -1).ShouldBe(21);
    }

    [Fact]
    public void when_mapping_a_fail_result_then_the_error_is_preserved()
    {
        Result<int, string> result = new Fail<int, string>("nope");

        Result<int, string> mapped = result.Map(value => value * 3);

        mapped.Match(_ => string.Empty, error => error).ShouldBe("nope");
    }
}
