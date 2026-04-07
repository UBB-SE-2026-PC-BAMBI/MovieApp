using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class ParticipationTests
{
    [Fact]
    public void Participation_PropertiesAndComputedKey_AreCorrect()
    {
        var participation = new Participation
        {
            Id = 1,
            UserId = 42,
            EventId = 100,
            Status = "Confirmed"
        };

        Assert.Equal(1, participation.Id);
        Assert.Equal(42, participation.UserId);
        Assert.Equal(100, participation.EventId);
        Assert.Equal("Confirmed", participation.Status);

        Assert.True((DateTime.UtcNow - participation.JoinedAt).TotalSeconds < 2);
        Assert.Equal("U42:E100", participation.ParticipationKey);
    }
}