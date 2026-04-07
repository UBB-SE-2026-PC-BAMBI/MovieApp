// <copyright file="FavoriteEvent.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Link entity representing an event marked as a favorite by a user.
/// Mirrors the style used by Participation (UserId + EventId).
/// </summary>
public sealed class FavoriteEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for the favorite link.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the unique identifier of the user who favorited the event.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the event that was favorited.
    /// </summary>
    required public int EventId { get; init; }

    /// <summary>
    /// Gets the date and time when the favorite link was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a composite key identifying the unique user-event pairing.
    /// </summary>
    public string FavoriteKey => $"U{this.UserId}:E{this.EventId}";
}
