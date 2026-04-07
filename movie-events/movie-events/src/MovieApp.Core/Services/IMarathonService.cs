// <copyright file="IMarathonService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Services;

using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for marathon progress, enrollment, and verification logic.
/// </summary>
public interface IMarathonService
{
    /// <summary>
    /// Gets the current week's marathons for the specified user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>A collection of marathons assigned for the current week.</returns>
    Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userIdentifier);

    /// <summary>
    /// Gets the current user's progress for a specific marathon.
    /// </summary>
    /// <param name="marathonIdentifier">The unique identifier of the marathon.</param>
    /// <returns>The progress details if found; otherwise, null.</returns>
    Task<MarathonProgress?> GetCurrentProgressAsync(int marathonIdentifier);

    /// <summary>
    /// Starts a marathon for the current user.
    /// </summary>
    /// <param name="marathonIdentifier">The unique identifier of the marathon to start.</param>
    /// <returns>True if the user successfully joined the marathon; otherwise, false.</returns>
    Task<bool> StartMarathonAsync(int marathonIdentifier);

    /// <summary>
    /// Updates aggregate quiz accuracy after a rapid-fire verification round.
    /// </summary>
    /// <param name="marathonIdentifier">The unique identifier of the marathon.</param>
    /// <param name="correctAnswersCount">The number of correct answers achieved in the quiz.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateQuizResultAsync(int marathonIdentifier, int correctAnswersCount);

    /// <summary>
    /// Logs a verified movie within a marathon.
    /// </summary>
    /// <param name="marathonIdentifier">The unique identifier of the marathon.</param>
    /// <param name="movieIdentifier">The unique identifier of the movie.</param>
    /// <param name="correctAnswersCount">The number of correct answers achieved for verification.</param>
    /// <returns>True if the movie was logged successfully; otherwise, false.</returns>
    Task<bool> LogMovieAsync(int marathonIdentifier, int movieIdentifier, int correctAnswersCount);

    Task<int> GetParticipantCountAsync(int marathonId);

    Task<int> GetMarathonMovieCountAsync(int marathonId);

    Task<bool> IsPrerequisiteCompletedAsync(int userId, int marathonId);

    Task<IEnumerable<Movie>> GetMoviesForMarathonAsync(int marathonId);

    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int marathonId);

    Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId);

    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId);
}