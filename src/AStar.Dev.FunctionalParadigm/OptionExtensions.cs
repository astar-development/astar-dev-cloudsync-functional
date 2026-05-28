namespace AStar.Dev.FunctionalParadigm;

public static class OptionExtensions
{
    public static Option<TResult> Tap<TResult>(this Option<TResult> option, Action<TResult> onSome, Action<string>? onNone = null)
    {
        if (option is Some<TResult> some)
        {
            onSome(some.Value);
            return some;
        }

        if (option is None<TResult> none)
        {
            onNone?.Invoke(none);
            return none;
        }

        throw new InvalidOperationException("Unexpected option type.");
    }

    public static Option<TMapped> Map<TResult, TMapped>(this Option<TResult> option, Func<TResult, TMapped> selector)
        => option switch
            {
                Some<TResult> some => new Some<TMapped>(selector(some.Value)),
                None<TResult> => new None<TMapped>(),
                _ => throw new InvalidOperationException("Unexpected option type.")
            };

    public static Option<TMapped> Bind<TResult, TMapped>(this Option<TResult> option, Func<TResult, Option<TMapped>> binder)
        => option switch
            {
                Some<TResult> some => binder(some.Value),
                None<TResult> => new None<TMapped>(),
                _ => throw new InvalidOperationException("Unexpected option type.")
            };

    public static TOut Match<TResult, TOut>(this Option<TResult> option, Func<TResult, TOut> onSome, Func<string, TOut> onNone)
        => option switch
            {
                Some<TResult> some => onSome(some.Value),
                None<TResult> none => onNone(none),
                _ => throw new InvalidOperationException("Unexpected option type.")
            };

    public static TOut Match<TResult, TOut>(this Option<TResult> option, Func<TResult, TOut> onSome)
        => option switch
            {
                Some<TResult> some => onSome(some.Value),
                None<TResult> none => new None<TOut>(),
                _ => throw new InvalidOperationException("Unexpected option type.")
            };
}