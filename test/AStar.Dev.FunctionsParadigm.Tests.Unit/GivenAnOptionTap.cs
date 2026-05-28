using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAnOptionTap
{
    [Fact]
    public void when_tapping_a_some_option_then_the_success_action_runs()
    {
        Option<int> result = new Some<int>(5);
        var observed = 0;

        result.Tap(value => observed = value * 2, _ => observed = -1);

        observed.ShouldBe(10);
    }

    [Fact]
    public void when_tapping_a_none_option_then_the_failure_action_runs()
    {
        Option<int> result = new None<int>();
        string? observed = null;

        result.Tap(_ => observed = "unexpected", error => observed = "error");

        observed.ShouldBe("error");
    }
}
