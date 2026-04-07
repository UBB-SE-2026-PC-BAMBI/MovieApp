// <copyright file="TriviaReward.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a redeemable trivia-wheel reward for a user.
/// </summary>
public sealed class TriviaReward
{
    /// <summary>
    /// Gets the reward identifier.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the user that owns the reward.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the reward has already been redeemed.
    /// </summary>
    public bool IsRedeemed { get; set; }

    /// <summary>
    /// Gets the creation timestamp of the reward.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the reward is available.
    /// </summary>
    public bool IsAvailable => !this.IsRedeemed;

    /// <summary>
    /// Marks the reward as redeemed.
    /// </summary>
    public void Redeem()
    {
        this.IsRedeemed = true;
    }
}
