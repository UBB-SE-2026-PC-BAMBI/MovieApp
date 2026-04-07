// <copyright file="Notification.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Simple in-app notification, typically tied to an event.
/// Stored per user and queryable per user.
/// </summary>
public sealed class Notification
{
    /// <summary>
    /// Gets or sets the unique identifier for the notification.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the unique identifier of the user who owns the notification.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the event the notification refers to.
    /// </summary>
    required public int EventId { get; init; }

    /// <summary>
    /// Gets the category type of the notification (e.g. "EventReminder").
    /// </summary>
    required public string Type { get; init; }

    /// <summary>
    /// Gets the text content of the notification message.
    /// </summary>
    required public string Message { get; init; }

    /// <summary>
    /// Gets or sets the current read/unread state of the notification.
    /// </summary>
    public NotificationState State { get; set; } = NotificationState.Unread;

    /// <summary>
    /// Gets the date and time when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}