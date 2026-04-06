// <copyright file="FavoriteEventService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Implements the favorite-event workflow on top of repository abstractions.
/// </summary>
public sealed class FavoriteEventService : IFavoriteEventService
{
    private readonly IFavoriteEventRepository favoriteEventRepository;
    private readonly IEventRepository eventRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteEventService"/> class.
    /// </summary>
    /// <param name="favoriteEventRepository">The repository for managing favorite links.</param>
    /// <param name="eventRepository">The repository for accessing event details.</param>
    public FavoriteEventService(
        IFavoriteEventRepository favoriteEventRepository,
        IEventRepository eventRepository)
    {
        this.favoriteEventRepository = favoriteEventRepository;
        this.eventRepository = eventRepository;
    }

    /// <summary>
    /// Adds an event to the user's favorite list.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event is already favorited or not found.</exception>
    public async Task AddFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        bool alreadyExists = await this.ExistsFavoriteAsync(userId, eventId, cancellationToken);
        if (alreadyExists)
        {
            throw new InvalidOperationException("Event is already favorited by this user.");
        }

        Event? favoriteEventDetails = await this.eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (favoriteEventDetails is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        await this.favoriteEventRepository.AddAsync(userId, eventId, cancellationToken);
    }

    /// <summary>
    /// Removes an event from the user's favorite list.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RemoveFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return this.favoriteEventRepository.RemoveAsync(userId, eventId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all favorite link records for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of favorite event links.</returns>
    public Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return this.favoriteEventRepository.FindByUserAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Checks if a specific event is already favorited by a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the favorite link exists; otherwise, false.</returns>
    public Task<bool> ExistsFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return this.favoriteEventRepository.ExistsAsync(userId, eventId, cancellationToken);
    }

    /// <summary>
    /// Gets the full event details for all items favorited by the user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of event objects.</returns>
    public async Task<IReadOnlyList<Event>> GetFavoriteEventsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FavoriteEvent> favoriteEventLinks = await this.favoriteEventRepository.FindByUserAsync(userId, cancellationToken);
        List<Event> favoriteEvents = new List<Event>();

        foreach (FavoriteEvent favoriteEventLink in favoriteEventLinks)
        {
            Event? favoriteEventDetails = await this.eventRepository.FindByIdAsync(favoriteEventLink.EventId, cancellationToken);
            if (favoriteEventDetails is not null)
            {
                favoriteEvents.Add(favoriteEventDetails);
            }
        }

        return favoriteEvents;
    }
}
