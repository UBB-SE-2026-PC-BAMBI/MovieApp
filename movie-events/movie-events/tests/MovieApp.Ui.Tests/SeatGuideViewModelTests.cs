using System.Collections.Generic;
using System.Linq;
using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class SeatGuideViewModelTests
{
    [Fact]
    public void Constructor_ValidCapacity_SetsTotalRowsAndColumns()
    {
        SeatGuideViewModel viewModel = new SeatGuideViewModel(50);

        Assert.Equal(50, viewModel.Seats.Count);
        Assert.Equal(5, viewModel.TotalRows);
        Assert.Equal(10, viewModel.TotalColumns);
    }

    [Fact]
    public void Constructor_CapacityNotDivisibleByTen_CreatesExactNumberOfSeats()
    {
        SeatGuideViewModel viewModel = new SeatGuideViewModel(54);

        Assert.Equal(54, viewModel.Seats.Count);
        Assert.Equal(6, viewModel.TotalRows);
    }

    [Fact]
    public void Constructor_WhenCalled_SetsFirstTwoRowsToPoorQuality()
    {
        SeatGuideViewModel viewModel = new SeatGuideViewModel(50);

        List<Seat> frontSeats = viewModel.Seats.Where(s => s.Row <= 2).ToList();

        Assert.NotEmpty(frontSeats);
        Assert.All(frontSeats, s => Assert.Equal(SeatQuality.Poor, s.Quality));
        Assert.All(frontSeats, s => Assert.False(s.IsSweetSpot));
    }

    [Fact]
    public void Constructor_WhenCalled_MarksCenterSeatsAsSweetSpots()
    {
        SeatGuideViewModel viewModel = new SeatGuideViewModel(50);

        List<Seat> sweetSpots = viewModel.Seats.Where(s => s.IsSweetSpot).ToList();

        Assert.NotEmpty(sweetSpots);
        Assert.All(sweetSpots, s => Assert.Equal(SeatQuality.Optimal, s.Quality));

        Assert.All(sweetSpots, s => Assert.True(s.Row is >= 3 and <= 4));
        Assert.All(sweetSpots, s => Assert.True(s.Column is >= 4 and <= 6));
    }

    [Fact]
    public void Constructor_WhenCalled_SetsRandomAvailabilityForSeats()
    {
        SeatGuideViewModel viewModel = new SeatGuideViewModel(200);

        List<Seat> unavailableSeats = viewModel.Seats.Where(s => !s.IsAvailable).ToList();

        Assert.NotEmpty(unavailableSeats);
    }
}