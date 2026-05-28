namespace AStar.Dev.FunctionalParadigm;

public abstract record Option<TResult>
{
    public static implicit operator TResult(Option<TResult> option) =>
        option switch
        {
            Some<TResult> some => some.Value,
            _ => default!
        };

    public static implicit operator string(Option<TResult> option) =>
        option switch
        {
            None<TResult> => "missing",
            Option<TResult>.None => "missing",
            _ => string.Empty
        };
        
    /// <summary>
    ///     Represents the absence of a value.
    /// </summary>
    public sealed record None : Option<TResult>
    {
        /// <summary>
        ///     A helper method to create an instance of <see cref="Option{T}.None" />
        /// </summary>
        public static readonly None Instance = new();

        private None()
        {
        }

        /// <summary>
        ///     Overrides the ToString method to return the type as a simple string.
        /// </summary>
        /// <returns>The overridden ToString</returns>
        public override string ToString() => "None";
    }
}

public record Some<TResult>(TResult Value) : Option<TResult>;

public record None<TResult>() : Option<TResult>;
