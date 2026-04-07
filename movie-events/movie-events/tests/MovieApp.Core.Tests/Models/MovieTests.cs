using Xunit;
using MovieApp.Core.Models.Movie;
using System.Collections.Generic;

namespace MovieApp.Core.Tests.Models;

public class MovieTests
{
    [Fact]
    public void Movie_Properties_AreSetCorrectly()
    {
        var movie = new Movie
        {
            Id = 1,
            Title = "Inception",
            Description = "A mind-bending thriller.",
            ReleaseYear = 2010,
            DurationMinutes = 148,
            Rating = 8.8,
            Genres = new List<Genre> { new Genre { Id = 1, Name = "Sci-Fi" } },
            Actors = new List<Actor> { new Actor { Id = 1, Name = "Leonardo DiCaprio" } },
            Directors = new List<Director> { new Director { Id = 1, Name = "Christopher Nolan" } }
        };

        Assert.Equal(1, movie.Id);
        Assert.Equal("Inception", movie.Title);
        Assert.Equal("A mind-bending thriller.", movie.Description);
        Assert.Equal(2010, movie.ReleaseYear);
        Assert.Equal(148, movie.DurationMinutes);
        Assert.Equal(8.8, movie.Rating);
        Assert.Single(movie.Genres);
        Assert.Single(movie.Actors);
        Assert.Single(movie.Directors);
    }
}