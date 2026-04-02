namespace MovieApp.Core.Models.Movie;

/// <summary>
/// Represents a valid (Genre, Actor, Director) reel combination
/// derived from a movie that has at least one active screening.
/// </summary>
public sealed class ReelCombination
{
    public required Genre Genre { get; init; }

    public required Actor Actor { get; init; }

    public required Director Director { get; init; }
}
