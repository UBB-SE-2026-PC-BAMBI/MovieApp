// <copyright file="AmbassadorProfile.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Referral code model with stable code storage.  Each user can become an ambassador with a permanent, unique referral code.
/// </summary>
public sealed class AmbassadorProfile
{
    /// <summary>
    /// Gets the unique identifier of the user who owns this ambassador profile.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets the permanent, unique referral code assigned to this ambassador.
    /// </summary>
    required public string PermanentCode { get; init; }

    /// <summary>
    /// Gets or sets the current balance of rewards earned through referrals.
    /// </summary>
    public int RewardBalance { get; set; }
}
