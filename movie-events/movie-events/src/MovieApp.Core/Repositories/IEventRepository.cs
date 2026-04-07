// <copyright file="IEventRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for movie events and screening metadata.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Retrieves all events stored in the system.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of all event entities.</returns>
    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves events filtered by their specific type category.
    /// </summary>
    /// <param name="eventType">The string representation of the event type.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of events matching the specified type.</returns>
    Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a specific event by its unique identifier.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The event entity if found; otherwise, null.</returns>
    Task<Event?> FindByIdAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new event to the persistence store.
    /// </summary>
    /// <param name="eventDetails">The event entity to create.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The newly created unique identifier for the event.</returns>
    Task<int> AddAsync(Event eventDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing event with new data values.
    /// </summary>
    /// <param name="eventDetails">The event entity containing updated values.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<bool> UpdateAsync(Event eventDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the enrollment count for a specific event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="newEnrollmentCount">The updated count of participants.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the enrollment was updated; otherwise, false.</returns>
    Task<bool> UpdateEnrollmentAsync(int eventId, int newEnrollmentCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a full update of an event entity.
    /// </summary>
    /// <param name="updatedEvent">The event entity to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an event from the persistence store.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to remove.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default);
}