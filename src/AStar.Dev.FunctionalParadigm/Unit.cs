namespace AStar.Dev.FunctionalParadigm;

/// <summary>Represents the absence of a meaningful return value.</summary>
public record Unit
{
    /// <summary>Gets the singleton default instance.</summary>
    public static Unit Default { get; } = new();
}
