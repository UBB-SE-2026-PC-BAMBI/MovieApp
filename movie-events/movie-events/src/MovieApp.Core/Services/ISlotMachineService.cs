// <copyright file="ISlotMachineService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;

/// <summary>
/// Defines the contract for slot machine operations, including spins, rewards, and metadata retrieval.
/// </summary>
public interface ISlotMachineService
{
    /// <summary>
    /// Executes a spin for the specified user and determines the outcome.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>A result object containing the reels' state and any matching events.</returns>
    Task<SlotMachineResult> SpinAsync(int userIdentifier);

    /// <summary>
    /// Gets the count of spins currently available for the user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>The number of available spins.</returns>
    Task<int> GetAvailableSpinsAsync(int userIdentifier);

    /// <summary>
    /// Retrieves the full spin and login state for a user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>An entity containing daily spins, bonuses, and streak data.</returns>
    Task<UserSpinData> GetUserSpinStateAsync(int userIdentifier);

    /// <summary>
    /// Grants a bonus spin to a user for participating in an event.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>True if a bonus spin was granted; otherwise, false.</returns>
    Task<bool> GrantBonusSpinForEventParticipationAsync(int userIdentifier);

    /// <summary>
    /// Records a login for the user and evaluates their current streak.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>True if the login was recorded successfully; otherwise, false.</returns>
    Task<bool> RecordLoginAndCheckStreakAsync(int userIdentifier);

    /// <summary>
    /// Grants an additional spin to a user who has maintained a valid login streak.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>True if a streak spin was granted; otherwise, false.</returns>
    Task<bool> GrantStreakSpinAsync(int userIdentifier);

    /// <summary>
    /// Retrieves a random genre from the available pool.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A random genre entity.</returns>
    Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a random actor from the available pool.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A random actor entity.</returns>
    Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a random director from the available pool.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A random director entity.</returns>
    Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available genres for the slot machine reels.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of genres.</returns>
    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available actors for the slot machine reels.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of actors.</returns>
    Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available directors for the slot machine reels.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of directors.</returns>
    Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves events that match a specific combination of genre, actor, and director.
    /// </summary>
    /// <param name="genreIdentifier">The unique identifier of the genre.</param>
    /// <param name="actorIdentifier">The unique identifier of the actor.</param>
    /// <param name="directorIdentifier">The unique identifier of the director.</param>
    /// <returns>A list of events matching all criteria.</returns>
    Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier);

    /// <summary>
    /// Finds a specific movie that matches the jackpot criteria for the given reel identifiers.
    /// </summary>
    /// <param name="genreIdentifier">The identifier of the genre.</param>
    /// <param name="actorIdentifier">The identifier of the actor.</param>
    /// <param name="directorIdentifier">The identifier of the director.</param>
    /// <returns>A movie entity if a jackpot match is found; otherwise, null.</returns>
    Task<Movie?> FindJackpotMovieAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier);

    /// <summary>
    /// Grants a specific movie discount to a user for hitting a jackpot.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="movieIdentifier">The unique identifier of the movie.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GrantJackpotDiscount(int userIdentifier, int movieIdentifier);
}