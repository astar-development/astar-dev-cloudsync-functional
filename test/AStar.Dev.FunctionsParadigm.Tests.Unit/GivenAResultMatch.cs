using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.FunctionsParadigm.Tests.Unit;

public class GivenAResultMatch
{
    [Fact]
    public void when_matching_an_ok_result_then_the_success_handler_is_used()
    {
        Result<int, string> result = new Ok<int, string>(8);

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        matched.ShouldBe("ok:8");
    }

    [Fact]
    public void when_matching_a_fail_result_then_the_failure_handler_is_used()
    {
        Result<int, string> result = new Fail<int, string>("bad");

        string matched = result.Match(value => $"ok:{value}", error => $"fail:{error}");

        matched.ShouldBe("fail:bad");
    }
    
    // ... existing code ...
    [Fact]
    public async Task when_task_result_is_fail_then_func_overload_returns_failure_value()
    {
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("err"));

        var result = await task.MatchAsync(
            ok => ok * 2,
            _ => -1);

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task when_task_result_is_ok_then_async_func_overload_returns_mapped_value()
    {
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Ok<int, string>(5));

        var result = await task.MatchAsync(
            ok => Task.FromResult(ok * 2),
            _ => Task.FromResult(-1));

        result.ShouldBe(10);
    }

    [Fact]
    public async Task when_task_result_is_fail_then_async_func_overload_returns_failure_value()
    {
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("err"));

        var result = await task.MatchAsync(
            ok => Task.FromResult(ok * 2),
            _ => Task.FromResult(-1));

        result.ShouldBe(-1);
    }

    [Fact]
    public async Task when_task_result_is_ok_then_async_func_overload_does_not_call_failure_branch()
    {
        var failureCalled = false;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Ok<int, string>(5));

        var result = await task.MatchAsync(
            ok => Task.FromResult(ok * 2),
            _ =>
            {
                failureCalled = true;
                return Task.FromResult(-1);
            });

        result.ShouldBe(10);
        failureCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task when_task_result_is_fail_then_async_func_overload_does_not_call_success_branch()
    {
        var successCalled = false;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("err"));

        var result = await task.MatchAsync(
            ok =>
            {
                successCalled = true;
                return Task.FromResult(ok * 2);
            },
            _ => Task.FromResult(-1));

        result.ShouldBe(-1);
        successCalled.ShouldBeFalse();
    }
}
