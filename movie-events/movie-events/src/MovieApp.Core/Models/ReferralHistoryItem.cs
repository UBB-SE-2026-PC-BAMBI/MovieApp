// <copyright file="ReferralHistoryItem.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents a historical record of a referral interaction for display purposes.
/// </summary>
public class ReferralHistoryItem
{
    /// <summary>
    /// Gets or sets the name of the friend who was referred.
    /// </summary>
    public string FriendName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the event the friend joined.
    /// </summary>
    public string EventTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the referral code was used.
    /// </summary>
    public DateTime UsedAt { get; set; }

    /// <summary>
    /// Gets a string representation of the usage date formatted for general display.
    /// </summary>
    public string FormattedDate => this.UsedAt.ToString("g");
}
