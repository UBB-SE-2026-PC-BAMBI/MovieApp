using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class MarathonDisplayItemTests
{
    [Fact]
    public void ToEvent_WhenJoinedAndHasPrerequisite_MapsToEliteMarathonWithRating()
    {

        var baseMarathon = new Marathon { Id = 1, Title = "Marvel Phase 1", PrerequisiteMarathonId = 99 };
        var displayItem = new MarathonDisplayItem
        {
            Marathon = baseMarathon,
            IsJoinedByUser = true,
            UserAccuracy = 85.0,
            ParticipantCount = 100,
            WeekEnd = new DateTime(2026, 12, 31)
        };

        var result = displayItem.ToEvent();

        Assert.Equal("Elite Marathon", result.LocationReference);
        Assert.Equal(4.2, result.HistoricalRating);
        Assert.Equal("Marvel Phase 1", result.Title);
        Assert.Equal(100, result.CurrentEnrollment);
    }

    [Fact]
    public void ToEvent_WhenNotJoinedAndNoPrerequisite_MapsToStandardMarathonWithZeroRating()
    {
        var baseMarathon = new Marathon { Id = 2, Title = "Pixar Classics" };
        var displayItem = new MarathonDisplayItem
        {
            Marathon = baseMarathon,
            IsJoinedByUser = false,
            UserAccuracy = 100.0,
            WeekEnd = new DateTime(2026, 1, 1)
        };

        var result = displayItem.ToEvent();

        Assert.Equal("Standard Marathon", result.LocationReference);
        Assert.Equal(0.0, result.HistoricalRating);
    }

    [Fact]
    public void ToEvent_WithOvercapAccuracy_ClampsRatingToFive()
    {
        var displayItem = new MarathonDisplayItem
        {
            Marathon = new Marathon { Id = 3, Title = "Star Wars" },
            IsJoinedByUser = true,
            UserAccuracy = 150.0
        };

        var result = displayItem.ToEvent();

        Assert.Equal(5.0, result.HistoricalRating);
    }
}