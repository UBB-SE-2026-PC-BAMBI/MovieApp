// <copyright file="Participation.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents a record of a user participating in a specific event.
/// </summary>
public sealed class Participation
{
    /// <summary>
    /// Gets the unique identifier for the participation record.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the participating user.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    required public int EventId { get; init; }

    /// <summary>
    /// Gets or sets the current status of the participation (e.g., "Confirmed").
    /// </summary>
    required public string Status { get; set; }

    /// <summary>
    /// Gets the date and time the user joined the event.
    /// </summary>
    public DateTime JoinedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a composite key identifying the unique user-event participation pairing.
    /// </summary>
    public string ParticipationKey => $"U{this.UserId}:E{this.EventId}";
}
