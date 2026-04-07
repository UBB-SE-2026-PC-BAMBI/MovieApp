using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class ReferralHistoryItemTests
{
    [Fact]
    public void ReferralHistoryItem_FormatsDateCorrectly()
    {
        var usedAtDate = new DateTime(2026, 4, 8, 15, 30, 0);
        var item = new ReferralHistoryItem
        {
            FriendName = "Alice",
            EventTitle = "Sci-Fi Marathon",
            UsedAt = usedAtDate
        };

        Assert.Equal("Alice", item.FriendName);
        Assert.Equal("Sci-Fi Marathon", item.EventTitle);
        Assert.Equal(usedAtDate, item.UsedAt);

        Assert.Equal(usedAtDate.ToString("g"), item.FormattedDate);
    }
}