// <copyright file="Event.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents a movie event or screening with capacity and enrollment details.
/// </summary>
public sealed class Event
{
    /// <summary>
    /// The default maximum capacity for an event when not otherwise specified.
    /// </summary>
    public const int DefaultMaxCapacity = 50;

    /// <summary>
    /// Gets the unique identifier for the event.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the title of the event.
    /// </summary>
    required public string Title { get; init; }

    /// <summary>
    /// Gets or sets the description of the event.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL for the event poster image.
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets the date and time the event occurs.
    /// </summary>
    required public DateTime EventDateTime { get; init; }

    /// <summary>
    /// Gets or sets the location reference for the event.
    /// </summary>
    required public string LocationReference { get; set; }

    /// <summary>
    /// Gets or sets the ticket price for the event.
    /// </summary>
    required public decimal TicketPrice { get; set; }

    /// <summary>
    /// Gets or sets the historical rating of the event.
    /// </summary>
    public double HistoricalRating { get; set; }

    /// <summary>
    /// Gets or sets the maximum capacity of the event.
    /// </summary>
    public int MaxCapacity { get; set; } = DefaultMaxCapacity;

    /// <summary>
    /// Gets or sets the current number of enrolled users.
    /// </summary>
    public int CurrentEnrollment { get; set; }

    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the user who created the event.
    /// </summary>
    required public int CreatorUserId { get; init; }

    /// <summary>
    /// Gets the number of available spots remaining.
    /// </summary>
    public int AvailableSpots => this.MaxCapacity - this.CurrentEnrollment;

    /// <summary>
    /// Gets a value indicating whether the event is available for enrollment.
    /// </summary>
    public bool IsAvailable => this.AvailableSpots > 0 && this.EventDateTime > DateTime.Now;

    /// <summary>
    /// Gets or sets the discount percentage applied to this event for the current user.
    /// </summary>
    public int DiscountPercentage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user has already joined this event.
    /// </summary>
    public bool IsJoined { get; set; }
}
