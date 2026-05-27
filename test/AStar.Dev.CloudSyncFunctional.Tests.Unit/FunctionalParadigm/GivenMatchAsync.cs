using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.FunctionalParadigm;

public class GivenMatchAsync
{
    [Fact]
    public async Task when_task_result_is_ok_then_success_action_is_called()
    {
        var called = false;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Ok<int, string>(42));

        await task.MatchAsync(
            ok => { called = true; },
            _ => { });

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task when_task_result_is_fail_then_failure_action_is_called()
    {
        var called = false;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("err"));

        await task.MatchAsync(
            _ => { },
            err => { called = true; });

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task when_task_result_is_ok_then_success_action_receives_correct_value()
    {
        int received = 0;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Ok<int, string>(42));

        await task.MatchAsync(
            ok => { received = ok; },
            _ => { });

        received.ShouldBe(42);
    }

    [Fact]
    public async Task when_task_result_is_fail_then_failure_action_receives_correct_error()
    {
        string? received = null;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("boom"));

        await task.MatchAsync(
            _ => { },
            err => { received = err; });

        received.ShouldBe("boom");
    }

    [Fact]
    public async Task when_task_result_is_ok_then_failure_branch_is_not_called()
    {
        var failureCalled = false;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Ok<int, string>(1));

        await task.MatchAsync(
            _ => { },
            _ => { failureCalled = true; });

        failureCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task when_task_result_is_fail_then_success_action_is_not_called()
    {
        var successCalled = false;
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("err"));

        await task.MatchAsync(
            _ => { successCalled = true; },
            _ => { });

        successCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task when_task_result_is_ok_then_func_overload_returns_mapped_value()
    {
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Ok<int, string>(5));

        var result = await task.MatchAsync(
            ok => ok * 2,
            _ => -1);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task when_task_result_is_fail_then_func_overload_returns_failure_value()
    {
        Task<Result<int, string>> task = Task.FromResult<Result<int, string>>(new Fail<int, string>("err"));

        var result = await task.MatchAsync(
            ok => ok * 2,
            _ => -1);

        result.ShouldBe(-1);
    }
}
