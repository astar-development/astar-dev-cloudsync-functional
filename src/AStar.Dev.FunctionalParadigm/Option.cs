namespace AStar.Dev.FunctionalParadigm;

public abstract record Option<TResult, TError> : Result<TResult, TError>
{
    public static implicit operator TResult(Option<TResult, TError> option) =>
        option switch
        {
            Some<TResult, TError> some => some.Value,
            _ => default!
        };

    public static implicit operator TError(Option<TResult, TError> option) =>
        option switch
        {
            None<TResult, TError> none => none.Error,
            _ => default!
        };
}

public record Some<TResult, TError>(TResult Value) : Option<TResult, TError>;

public record None<TResult, TError>(TError Error) : Option<TResult, TError>;