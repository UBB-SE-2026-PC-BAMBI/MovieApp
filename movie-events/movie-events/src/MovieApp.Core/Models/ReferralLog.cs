// <copyright file="ReferralLog.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Referral interaction model that tracks valid referral code usage.
/// Records when a referred user joins an event using an ambassador's code.
/// </summary>
public sealed class ReferralLog
{
    /// <summary>
    /// Gets the unique identifier for the referral log entry.
    /// </summary>
    required public int LogId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the ambassador who provided the code.
    /// </summary>
    required public int AmbassadorId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user who was referred.
    /// </summary>
    required public int ReferredUserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the event the referred user joined.
    /// </summary>
    required public int EventId { get; init; }

    /// <summary>
    /// Gets the date and time when the referral interaction was logged.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
