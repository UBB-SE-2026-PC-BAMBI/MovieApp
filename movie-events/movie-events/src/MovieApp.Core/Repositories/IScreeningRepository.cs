// <copyright file="IScreeningRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for movie screenings linked to events.
/// </summary>
public interface IScreeningRepository
{
    /// <summary>
    /// Retrieves all screenings associated with a specific event.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of screenings for the event.</returns>
    Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all screenings associated with a specific movie.
    /// </summary>
    /// <param name="movieIdentifier">The unique identifier of the movie.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of screenings for the movie.</returns>
    Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new screening record to the persistence store.
    /// </summary>
    /// <param name="screening">The screening entity to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Screening screening, CancellationToken cancellationToken = default);
}