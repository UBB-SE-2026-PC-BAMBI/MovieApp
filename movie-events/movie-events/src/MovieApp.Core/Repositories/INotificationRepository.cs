// <copyright file="INotificationRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for user notifications.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Adds a new notification to the persistence store.
    /// </summary>
    /// <param name="notification">The notification entity to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a notification from the persistence store.
    /// </summary>
    /// <param name="notificationIdentifier">The unique identifier of the notification to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(int notificationIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications for a specific user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of notifications belonging to the user.</returns>
    Task<IReadOnlyList<Notification>> FindByUserAsync(int userIdentifier, CancellationToken cancellationToken = default);
}
