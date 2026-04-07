// <copyright file="IMovieRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;

/// <summary>
/// Provides persistence operations for movies, genres, actors, and directors.
/// </summary>
public interface IMovieRepository
{
    /// <summary>
    /// Retrieves all available genres.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of genre entities.</returns>
    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available actors.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of actor entities.</returns>
    Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available directors.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of director entities.</returns>
    Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns movies that match all of the given criteria (AND logic).
    /// </summary>
    /// <param name="genreIdentifier">The unique identifier of the genre.</param>
    /// <param name="actorIdentifier">The unique identifier of the actor.</param>
    /// <param name="directorIdentifier">The unique identifier of the director.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of movies matching the criteria.</returns>
    Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns movies that match any of the given criteria (OR logic).
    /// </summary>
    /// <param name="genreIdentifier">The unique identifier of the genre.</param>
    /// <param name="actorIdentifier">The unique identifier of the actor.</param>
    /// <param name="directorIdentifier">The unique identifier of the director.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of movies matching any of the criteria.</returns>
    Task<IReadOnlyList<Movie>> FindMoviesByAnyCriteriaAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the identifiers of events where a specific movie is being screened.
    /// </summary>
    /// <param name="movieIdentifier">The unique identifier of the movie.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of event identifiers.</returns>
    Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all valid (Genre, Actor, Director) combinations from movies
    /// that have at least one screening in a future event.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of valid reel combinations.</returns>
    Task<IReadOnlyList<ReelCombination>> GetValidReelCombinationsAsync(CancellationToken cancellationToken = default);
}
