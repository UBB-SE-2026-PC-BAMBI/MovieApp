// <copyright file="MarathonProgress.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Tracks a user's progress within a specific marathon.
/// </summary>
public sealed class MarathonProgress
{
    /// <summary>
    /// Gets the participant user identifier.
    /// </summary>
    required public int UserId { get; init; }

    /// <summary>
    /// Gets the marathon identifier.
    /// </summary>
    required public int MarathonId { get; init; }

    /// <summary>
    /// Gets or sets when the user joined the marathon.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the user's aggregate trivia accuracy percentage.
    /// </summary>
    public double TriviaAccuracy { get; set; }

    /// <summary>
    /// Gets or sets how many movies have been successfully verified.
    /// </summary>
    public int CompletedMoviesCount { get; set; }

    /// <summary>
    /// Gets or sets when the marathon was finished.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether the marathon has been completed.
    /// </summary>
    public bool IsCompleted => this.FinishedAt.HasValue;
}
