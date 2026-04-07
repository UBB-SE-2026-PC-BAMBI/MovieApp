using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class TriviaRewardTests
{
    [Fact]
    public void Redeem_UpdatesStateCorrectly()
    {
        var reward = new TriviaReward
        {
            Id = 1,
            UserId = 1,
            IsRedeemed = false
        };

        Assert.True(reward.IsAvailable);

        reward.Redeem();

        Assert.True(reward.IsRedeemed);
        Assert.False(reward.IsAvailable);
    }
}