using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAResultTap
{
    [Fact]
    public void when_tapping_an_ok_result_then_the_success_action_runs()
    {
        Result<int, string> result = new Ok<int, string>(5);
        var observed = 0;

        result.Tap(value => observed = value * 2);

        Assert.Equal(10, observed);
    }

    [Fact]
    public void when_tapping_a_fail_result_then_the_failure_action_runs()
    {
        Result<int, string> result = new Fail<int, string>("boom");
        string? observed = null;

        result.Tap(_ => observed = "unexpected", error => observed = error);

        Assert.Equal("boom", observed);
    }
}