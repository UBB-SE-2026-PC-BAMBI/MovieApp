using Xunit;
using MovieApp.Core.Models;

namespace MovieApp.Core.Tests.Models;

public class MarathonMovieItemTests
{
    [Fact]
    public void MarathonMovieItem_WhenNotVerified_ComputesPropertiesCorrectly()
    {
        var item = new MarathonMovieItem
        {
            MovieId = 1,
            Title = "The Matrix",
            IsVerified = false
        };

        Assert.Equal(1, item.MovieId);
        Assert.Equal("The Matrix", item.Title);

        Assert.Equal("Not verified", item.StatusText);
        Assert.True(item.CanLog);
        Assert.Equal(0.0, item.IsVerifiedOpacity);
        Assert.Equal(1.0, item.CanLogOpacity);
    }

    [Fact]
    public void MarathonMovieItem_WhenVerified_ComputesPropertiesCorrectly()
    {
        var item = new MarathonMovieItem
        {
            MovieId = 2,
            Title = "Inception",
            IsVerified = true
        };

        Assert.Equal(2, item.MovieId);
        Assert.Equal("Inception", item.Title);

        Assert.Equal("Verified", item.StatusText);
        Assert.False(item.CanLog);
        Assert.Equal(1.0, item.IsVerifiedOpacity);
        Assert.Equal(0.0, item.CanLogOpacity);
    }
}