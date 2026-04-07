using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class EventTests
{
    [Fact]
    public void AvailableSpots_IsCalculatedCorrectly()
    {
        var ev = new Event
        {
            Id = 1,
            Title = "Test",
            LocationReference = "Ref",
            TicketPrice = 0,
            EventDateTime = DateTime.UtcNow,
            CreatorUserId = 1,
            MaxCapacity = 100,
            CurrentEnrollment = 40
        };

        Assert.Equal(60, ev.AvailableSpots);
        Assert.Equal(Event.DefaultMaxCapacity, new Event { Id = 2, Title = "", LocationReference = "", TicketPrice = 0, EventDateTime = DateTime.UtcNow, CreatorUserId = 1 }.MaxCapacity);
    }

    [Fact]
    public void IsAvailable_ChecksBothCapacityAndDate()
    {
        var validEvent = new Event { Id = 1, Title = "", LocationReference = "", TicketPrice = 0, CreatorUserId = 1, MaxCapacity = 10, CurrentEnrollment = 5, EventDateTime = DateTime.Now.AddDays(1) };

        var pastEvent = new Event { Id = 2, Title = "", LocationReference = "", TicketPrice = 0, CreatorUserId = 1, MaxCapacity = 10, CurrentEnrollment = 5, EventDateTime = DateTime.Now.AddDays(-1) };

        var fullEvent = new Event { Id = 3, Title = "", LocationReference = "", TicketPrice = 0, CreatorUserId = 1, MaxCapacity = 10, CurrentEnrollment = 10, EventDateTime = DateTime.Now.AddDays(1) };

        Assert.True(validEvent.IsAvailable);
        Assert.False(pastEvent.IsAvailable);
        Assert.False(fullEvent.IsAvailable);
    }
}