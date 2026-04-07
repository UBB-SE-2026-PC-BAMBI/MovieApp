// <copyright file="IPriceWatcherRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for price-watching workflows.
/// </summary>
public interface IPriceWatcherRepository
{
    /// <summary>
    /// Retrieves all events currently being watched for price changes.
    /// </summary>
    /// <returns>A list of watched event entries.</returns>
    Task<List<WatchedEvent>> GetAllWatchedEventsAsync();

    /// <summary>
    /// Adds a new event to the price-watching list.
    /// </summary>
    /// <param name="watchedEvent">The watched event details.</param>
    /// <returns>True if the event was added successfully; otherwise, false.</returns>
    Task<bool> AddWatchAsync(WatchedEvent watchedEvent);

    /// <summary>
    /// Removes an event from the price-watching list.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveWatchAsync(int eventIdentifier);

    /// <summary>
    /// Retrieves specific watch details for an event.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <returns>The watched event record if found; otherwise, null.</returns>
    Task<WatchedEvent?> GetWatchAsync(int eventIdentifier);

    /// <summary>
    /// Determines whether an event is already being watched.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <returns>True if the event is being watched; otherwise, false.</returns>
    Task<bool> IsWatchingAsync(int eventIdentifier);
}