namespace MovieApp.Core.Models;

/// <summary>
/// Represents a screening that maps a movie to an event with a specific screening time.
/// </summary>
public sealed class Screening
{
    public required int Id { get; init; }

    public required int EventId { get; init; }

    public required int MovieId { get; init; }

    public required DateTime ScreeningTime { get; init; }
}
