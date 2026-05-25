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

    [Fact]
    public void when_mapping_an_ok_result_then_the_value_is_transformed()
    {
        Result<int, string> result = new Ok<int, string>(7);

        Result<int, string> mapped = result.Map(value => value * 3);

        Assert.Equal(21, mapped.Match(value => value, _ => -1));
    }

    [Fact]
    public void when_mapping_a_fail_result_then_the_error_is_preserved()
    {
        Result<int, string> result = new Fail<int, string>("nope");

        Result<int, string> mapped = result.Map(value => value * 3);

        Assert.Equal("nope", mapped.Match(_ => string.Empty, error => error));
    }

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

    [Fact]
    public void when_matching_an_ok_result_then_the_success_handler_is_used()
    {
        Result<int, string> result = new Ok<int, string>(8);

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        Assert.Equal("ok:8", matched);
    }

    [Fact]
    public void when_matching_a_fail_result_then_the_failure_handler_is_used()
    {
        Result<int, string> result = new Fail<int, string>("bad");

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        Assert.Equal("fail:bad", matched);
    }
}
