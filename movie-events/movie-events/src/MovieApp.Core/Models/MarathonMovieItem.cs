// <copyright file="MarathonMovieItem.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Represents a specific movie within a marathon, including its verification status for the user.
/// </summary>
public sealed class MarathonMovieItem
{
    private const double FullVisibility = 1.0;
    private const double ZeroVisibility = 0.0;

    /// <summary>
    /// Gets the unique identifier for the movie.
    /// </summary>
    required public int MovieId { get; init; }

    /// <summary>
    /// Gets the title of the movie.
    /// </summary>
    required public string Title { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has verified watching this movie.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Gets the display text for the verification status.
    /// </summary>
    public string StatusText => this.IsVerified ? "Verified" : "Not verified";

    /// <summary>
    /// Gets a value indicating whether the user is eligible to log this movie.
    /// </summary>
    public bool CanLog => !this.IsVerified;

    /// <summary>
    /// Gets the opacity value for the "Verified" visual indicator.
    /// </summary>
    public double IsVerifiedOpacity => this.IsVerified ? FullVisibility : ZeroVisibility;

    /// <summary>
    /// Gets the opacity value for the "Log Movie" action button.
    /// </summary>
    public double CanLogOpacity => this.IsVerified ? ZeroVisibility : FullVisibility;
}