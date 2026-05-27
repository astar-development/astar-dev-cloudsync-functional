namespace AStar.Dev.FunctionalParadigm;

/// <summary>Extension methods for <see cref="Result{TResult, TError}"/>.</summary>
public static class ResultExtensions
{
    /// <summary>Applies a side-effect to the current result without changing it.</summary>
    /// <typeparam name="TResult">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="onSuccess">Action to invoke on success.</param>
    /// <param name="onFailure">Optional action to invoke on failure.</param>
    /// <returns>The original result unchanged.</returns>
    public static Result<TResult, TError> Tap<TResult, TError>(this Result<TResult, TError> result, Action<TResult> onSuccess, Action<TError>? onFailure = null)
    {
        switch (result)
        {
            case Ok<TResult, TError> ok:
                onSuccess(ok.Value);
                return ok;

            case Fail<TResult, TError> fail:
                onFailure?.Invoke(fail.Error);
                return fail;

            default:
                throw new InvalidOperationException("Unexpected result type.");
        }
    }

    /// <summary>Transforms the success value using the provided selector.</summary>
    /// <typeparam name="TResult">The input success type.</typeparam>
    /// <typeparam name="TMapped">The output success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="selector">The transformation function.</param>
    /// <returns>A new result with the transformed value, or the original failure.</returns>
    public static Result<TMapped, TError> Map<TResult, TMapped, TError>(this Result<TResult, TError> result, Func<TResult, TMapped> selector)
    {
        return result switch
        {
            Ok<TResult, TError> ok => new Ok<TMapped, TError>(selector(ok.Value)),
            Fail<TResult, TError> fail => new Fail<TMapped, TError>(fail.Error),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    /// <summary>Chains an operation that also returns a result, propagating failure.</summary>
    /// <typeparam name="TResult">The input success type.</typeparam>
    /// <typeparam name="TMapped">The output success type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">The chained operation.</param>
    /// <returns>The result of the chained operation, or the original failure.</returns>
    public static Result<TMapped, TError> Bind<TResult, TMapped, TError>(this Result<TResult, TError> result, Func<TResult, Result<TMapped, TError>> binder)
    {
        return result switch
        {
            Ok<TResult, TError> ok => binder(ok.Value),
            Fail<TResult, TError> fail => new Fail<TMapped, TError>(fail.Error),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    /// <summary>Collapses the result to a single output value by handling both cases.</summary>
    /// <typeparam name="TResult">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Function invoked on success.</param>
    /// <param name="onFailure">Function invoked on failure.</param>
    /// <returns>The value produced by whichever branch was taken.</returns>
    public static TOut Match<TResult, TError, TOut>(this Result<TResult, TError> result, Func<TResult, TOut> onSuccess, Func<TError, TOut> onFailure)
    {
        return result switch
        {
            Ok<TResult, TError> ok => onSuccess(ok.Value),
            Fail<TResult, TError> fail => onFailure(fail.Error),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    /// <summary>Asynchronously handles both result cases as side-effects.</summary>
    /// <typeparam name="TResult">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="taskResult">The task producing the result.</param>
    /// <param name="onSuccess">Action invoked on success.</param>
    /// <param name="onFailure">Action invoked on failure.</param>
    /// <returns>A task that completes after the appropriate action runs.</returns>
    public static async Task MatchAsync<TResult, TError>(this Task<Result<TResult, TError>> taskResult, Action<TResult> onSuccess, Action<TError> onFailure)
    {
        var result = await taskResult.ConfigureAwait(false);
        switch (result)
        {
            case Ok<TResult, TError> ok:
                onSuccess(ok.Value);
                break;
            case Fail<TResult, TError> fail:
                onFailure(fail.Error);
                break;
        }
    }

    /// <summary>Asynchronously collapses the result to a single output value by handling both cases.</summary>
    /// <typeparam name="TResult">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="taskResult">The task producing the result.</param>
    /// <param name="onSuccess">Function invoked on success.</param>
    /// <param name="onFailure">Function invoked on failure.</param>
    /// <returns>A task that produces the value from whichever branch was taken.</returns>
    public static async Task<TOut> MatchAsync<TResult, TError, TOut>(this Task<Result<TResult, TError>> taskResult, Func<TResult, TOut> onSuccess, Func<TError, TOut> onFailure)
    {
        var result = await taskResult.ConfigureAwait(false);

        return result.Match(onSuccess, onFailure);
    }
}
