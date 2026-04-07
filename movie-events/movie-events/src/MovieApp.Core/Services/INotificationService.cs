// <copyright file="INotificationService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Coordinates notification creation and retrieval for favorited events.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Generates a price-drop notification using a supplied event title.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="eventTitle">The title of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GeneratePriceDropNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a seats-available notification using a supplied event title.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="eventTitle">The title of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GenerateSeatsAvailableNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications for a specific user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of notifications.</returns>
    Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific notification from the system.
    /// </summary>
    /// <param name="notificationIdentifier">The unique identifier of the notification.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveNotificationAsync(int notificationIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications for a specific user identifier.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of notifications.</returns>
    Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notifications when a price actually decreases.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="oldPrice">The price of the event before the decrease.</param>
    /// <param name="newPrice">The newly decreased price.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifyPriceDropAsync(int eventIdentifier, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notifications when seats become available for an event.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="newCapacity">The updated number of available seats.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NotifySeatsAvailableAsync(int eventIdentifier, int newCapacity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read or removes it from the active user list.
    /// </summary>
    /// <param name="notificationIdentifier">The unique identifier of the notification.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkAsReadOrRemoveAsync(int notificationIdentifier, CancellationToken cancellationToken = default);
}
