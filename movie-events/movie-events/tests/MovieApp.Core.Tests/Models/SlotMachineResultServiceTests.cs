using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieApp.Core.Services;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Tests.Models;

public class SlotMachineResultServiceTests
{
    [Fact]
    public async Task PrepareSpinResultAsync_WithJackpotMovie_AppliesDiscount()
    {
        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        var service = new SlotMachineResultService(mockDiscountRepo.Object);

        var genre = new Genre();
        var actor = new Actor();
        var director = new Director();
        var events = new List<Event>();
        var jackpotMovie = new Movie();

        var result = await service.PrepareSpinResultAsync(123, genre, actor, director, events, jackpotMovie);

        Assert.NotNull(result);
        Assert.Equal(genre, result.Genre);
        Assert.Equal(actor, result.Actor);
        Assert.Equal(director, result.Director);
        Assert.Equal(events, result.MatchingEvents);

        Assert.Equal(jackpotMovie, result.JackpotMovie);
        Assert.True(result.JackpotDiscountApplied);
        Assert.Equal(70, result.DiscountPercentage);
    }

    [Fact]
    public async Task PrepareSpinResultAsync_WithoutJackpotMovie_DoesNotApplyDiscount()
    {
        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        var service = new SlotMachineResultService(mockDiscountRepo.Object);

        var genre = new Genre();
        var actor = new Actor();
        var director = new Director();
        var events = new List<Event>();

        var result = await service.PrepareSpinResultAsync(123, genre, actor, director, events, null);

        Assert.NotNull(result);

        Assert.Null(result.JackpotMovie);
        Assert.False(result.JackpotDiscountApplied);
        Assert.Equal(0, result.DiscountPercentage);
    }

    [Fact]
    public void SlotMachineResult_InitializesCollectionsCorrectly()
    {
        var result = new SlotMachineResult();

        Assert.NotNull(result.Genre);
        Assert.NotNull(result.Actor);
        Assert.NotNull(result.Director);
        Assert.NotNull(result.MatchingEvents);
        Assert.Empty(result.MatchingEvents);
        Assert.NotNull(result.JackpotEventIds);
        Assert.Empty(result.JackpotEventIds);
    }
}