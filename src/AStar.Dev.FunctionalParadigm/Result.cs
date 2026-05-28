namespace AStar.Dev.FunctionalParadigm;

public abstract record Result<TResult, TError>
{
    public static implicit operator Result<TResult, TError>(TResult value) => new Ok<TResult, TError>(value);
    public static implicit operator Result<TResult, TError>(TError error) => new Fail<TResult, TError>(error);

    public static implicit operator TResult(Result<TResult, TError> result) =>
        result switch
        {
            Ok<TResult, TError> ok => ok.Value,
            _ => default!
        };

    public static implicit operator TError(Result<TResult, TError> result) =>
        result switch
        {
            Fail<TResult, TError> fail => fail.Error,
            _ => default!
        };
}

public record Ok<TResult, TError>(TResult Value) : Result<TResult, TError>;
public record Fail<TResult, TError>(TError Error) : Result<TResult, TError>;
