namespace AStar.Dev.FunctionalParadigm;

public static class OptionExtensions
{
    public static Option<TResult, TError> Tap<TResult, TError>(
        this Option<TResult, TError> option,
        Action<TResult> onSome,
        Action<TError>? onNone = null)
    {
        switch (option)
        {
            case Some<TResult, TError> some:
                onSome(some.Value);
                return some;

            case None<TResult, TError> none:
                onNone?.Invoke(none.Error);
                return none;

            default:
                throw new InvalidOperationException("Unexpected option type.");
        }
    }

    public static Option<TMapped, TError> Map<TResult, TMapped, TError>(
        this Option<TResult, TError> option,
        Func<TResult, TMapped> selector)
    {
        return option switch
        {
            Some<TResult, TError> some => new Some<TMapped, TError>(selector(some.Value)),
            None<TResult, TError> none => new None<TMapped, TError>(none.Error),
            _ => throw new InvalidOperationException("Unexpected option type.")
        };
    }

    public static Option<TMapped, TError> Bind<TResult, TMapped, TError>(
        this Option<TResult, TError> option,
        Func<TResult, Option<TMapped, TError>> binder)
    {
        return option switch
        {
            Some<TResult, TError> some => binder(some.Value),
            None<TResult, TError> none => new None<TMapped, TError>(none.Error),
            _ => throw new InvalidOperationException("Unexpected option type.")
        };
    }

    public static TOut Match<TResult, TError, TOut>(
        this Option<TResult, TError> option,
        Func<TResult, TOut> onSome,
        Func<TError, TOut> onNone)
    {
        return option switch
        {
            Some<TResult, TError> some => onSome(some.Value),
            None<TResult, TError> none => onNone(none.Error),
            _ => throw new InvalidOperationException("Unexpected option type.")
        };
    }
}