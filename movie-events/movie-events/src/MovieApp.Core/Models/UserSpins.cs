// <copyright file="UserSpins.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Stores user spin-related data such as daily spins, bonuses, and login tracking.
/// </summary>
public sealed class UserSpinData
{
    private const int YesterdayDayOffset = -1;
    private const int InitialStreakValue = 1;

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary
    required public int UserId { get; init; }

    /// <summary>
    /// Gets or sets the number of free daily spins remaining.
    /// </summary>
    public int DailySpinsRemaining { get; set; }

    /// <summary>
    /// Gets or sets the count of additional bonus spins earned.
    /// </summary>
    public int BonusSpins { get; set; }

    /// <summary>
    /// Gets or sets the last time the slot machine spins were reset.
    /// </summary>
    public DateTime LastSlotSpinReset { get; set; }

    /// <summary>
    /// Gets or sets the last time the trivia wheel spins were reset.
    /// </summary>
    public DateTime LastTriviaSpinReset { get; set; }

    /// <summary>
    /// Gets or sets the consecutive number of days the user has logged in.
    /// </summary>
    public int LoginStreak { get; set; }

    /// <summary>
    /// Gets or sets the last recorded login date.
    /// </summary>
    public DateTime LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the number of spins earned from event participation today.
    /// </summary>
    public int EventSpinRewardsToday { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has any spins available to use.
    /// </summary>
    public bool CanSpin => this.DailySpinsRemaining > 0 || this.BonusSpins > 0;

    /// <summary>
    /// Resets daily spins and event reward counters.
    /// </summary>
    /// <param name="defaultDailySpins">The standard amount of spins granted daily.</param>
    public void ResetDailySpins(int defaultDailySpins)
    {
        this.DailySpinsRemaining = defaultDailySpins;
        this.EventSpinRewardsToday = 0;
        this.LastSlotSpinReset = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the login streak based on the elapsed time since the last login.
    /// </summary>
    public void UpdateLoginStreak()
    {
        DateTime currentUtcDate = DateTime.UtcNow.Date;
        DateTime lastRecordedDate = this.LastLoginDate.Date;

        if (lastRecordedDate == currentUtcDate.AddDays(YesterdayDayOffset))
        {
            this.LoginStreak++;
        }
        else if (lastRecordedDate != currentUtcDate)
        {
            this.LoginStreak = InitialStreakValue;
        }

        lastRecordedDate = currentUtcDate;
    }
}