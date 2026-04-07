// <copyright file="IFavoriteEventService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Coordinates favorite-event workflows for the UI.
/// </summary>
public interface IFavoriteEventService
{
    /// <summary>
    /// Adds an event to the specified user's favorites.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an event from the specified user's favorites.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets favorite link rows for the specified user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of favorite event link entities.</returns>
    Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user has already favorited an event.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the event is favorited; otherwise, false.</returns>
    Task<bool> ExistsFavoriteAsync(int userIdentifier, int eventIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full event details for all items favorited by the specified user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of event entities.</returns>
    Task<IReadOnlyList<Event>> GetFavoriteEventsByUserIdAsync(int userIdentifier, CancellationToken cancellationToken = default);
}
