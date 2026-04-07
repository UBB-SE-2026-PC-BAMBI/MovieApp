using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class FavoriteEventTests
{
    [Fact]
    public void FavoriteEvent_PropertiesAndComputedKey_AreCorrect()
    {
        var fav = new FavoriteEvent
        {
            Id = 1,
            UserId = 7,
            EventId = 42
        };

        Assert.Equal(1, fav.Id);
        Assert.Equal(7, fav.UserId);
        Assert.Equal(42, fav.EventId);
        Assert.Equal("U7:E42", fav.FavoriteKey);
        Assert.True((DateTime.UtcNow - fav.CreatedAt).TotalSeconds < 2);
    }
}