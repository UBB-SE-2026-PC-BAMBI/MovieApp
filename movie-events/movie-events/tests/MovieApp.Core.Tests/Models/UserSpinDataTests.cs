using Xunit;
using MovieApp.Core.Models;
using System;

namespace MovieApp.Core.Tests.Models;

public class UserSpinDataTests
{
    [Fact]
    public void CanSpin_IsEvaluatedCorrectly()
    {
        var noSpins = new UserSpinData { UserId = 1, DailySpinsRemaining = 0, BonusSpins = 0 };
        var onlyDaily = new UserSpinData { UserId = 2, DailySpinsRemaining = 1, BonusSpins = 0 };
        var onlyBonus = new UserSpinData { UserId = 3, DailySpinsRemaining = 0, BonusSpins = 1 };

        Assert.False(noSpins.CanSpin);
        Assert.True(onlyDaily.CanSpin);
        Assert.True(onlyBonus.CanSpin);
    }

    [Fact]
    public void ResetDailySpins_AppliesCorrectDefaults()
    {
        var data = new UserSpinData { UserId = 1, EventSpinRewardsToday = 5 };

        data.ResetDailySpins(3);

        Assert.Equal(3, data.DailySpinsRemaining);
        Assert.Equal(0, data.EventSpinRewardsToday);
        Assert.True((DateTime.UtcNow - data.LastSlotSpinReset).TotalSeconds < 2);
    }

    [Fact]
    public void UpdateLoginStreak_OnMissedDays_ResetsStreak()
    {
        var data = new UserSpinData
        {
            UserId = 1,
            LastLoginDate = DateTime.UtcNow.AddDays(-2),
            LoginStreak = 5
        };

        data.UpdateLoginStreak();

        Assert.Equal(1, data.LoginStreak);
    }

    [Fact]
    public void UpdateLoginStreak_OnSameDay_PreservesStreak()
    {
        var data = new UserSpinData
        {
            UserId = 1,
            LastLoginDate = DateTime.UtcNow,
            LoginStreak = 5
        };

        data.UpdateLoginStreak();

        Assert.Equal(5, data.LoginStreak);
    }
}