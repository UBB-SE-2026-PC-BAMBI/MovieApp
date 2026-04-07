// <copyright file="IFavoriteEventRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for user favorite-event links.
/// </summary>
public interface IFavoriteEventRepository
{
    /// <summary>
    /// Creates a favorite link between a user and an event.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a favorite link between a user and an event.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all favorite links owned by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of favorite event records.</returns>
    Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific favorite link already exists for a user-event pair.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the favorite link exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of user identifiers that have favorited a specific event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of user identifiers.</returns>
    Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all favorite links pointing to a specific event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of favorite event records.</returns>
    Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default);
}
