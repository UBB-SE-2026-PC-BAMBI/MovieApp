namespace MovieApp.Core.Models.Movie;

public sealed class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public int DurationMinutes { get; set; }

    public List<Genre> Genres { get; set; } = [];

    public List<Actor> Actors { get; set; } = [];

    public List<Director> Directors { get; set; } = [];

    public double Rating { get; set; }
}
