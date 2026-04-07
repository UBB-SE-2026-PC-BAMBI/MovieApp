using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class ScreeningTests
{
    [Fact]
    public void Screening_Properties_AreSetCorrectly()
    {
        var screenTime = DateTime.UtcNow.AddDays(1);
        var screening = new Screening
        {
            Id = 1,
            EventId = 2,
            MovieId = 3,
            ScreeningTime = screenTime
        };

        Assert.Equal(1, screening.Id);
        Assert.Equal(2, screening.EventId);
        Assert.Equal(3, screening.MovieId);
        Assert.Equal(screenTime, screening.ScreeningTime);
    }
}