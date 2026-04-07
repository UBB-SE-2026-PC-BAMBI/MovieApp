// <copyright file="LeaderboardEntry.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents an individual entry in the marathon leaderboard, containing user performance metrics.
/// </summary>
public sealed class LeaderboardEntry
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    required public string Username { get; init; }

    /// <summary>
    /// Gets the total number of movies successfully completed by the user.
    /// </summary>
    public int CompletedMoviesCount { get; init; }

    /// <summary>
    /// Gets the overall trivia accuracy percentage for the user.
    /// </summary>
    public double TriviaAccuracy { get; init; }

    /// <summary>
    /// Gets the timestamp when the user completed their final marathon task, if applicable.
    /// </summary>
    public DateTime? FinishedAt { get; init; }
}