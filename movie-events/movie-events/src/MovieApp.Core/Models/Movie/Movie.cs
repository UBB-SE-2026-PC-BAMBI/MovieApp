// <copyright file="Movie.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models.Movie;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a movie entity containing metadata, cast, and classification details.
/// </summary>
public sealed class Movie
{
    /// <summary>
    /// Gets or sets the unique identifier for the movie.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the movie.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptive summary of the movie plot.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year the movie was officially released.
    /// </summary>
    public int ReleaseYear { get; set; }

    /// <summary>
    /// Gets or sets the total running time of the movie in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the collection of genres associated with this movie.
    /// </summary>
    public List<Genre> Genres { get; set; } = new List<Genre>();

    /// <summary>
    /// Gets or sets the collection of actors who performed in the movie.
    /// </summary>
    public List<Actor> Actors { get; set; } = new List<Actor>();

    /// <summary>
    /// Gets or sets the collection of directors who managed the movie production.
    /// </summary>
    public List<Director> Directors { get; set; } = new List<Director>();

    /// <summary>
    /// Gets or sets the average viewer or critic rating for the movie.
    /// </summary>
    public double Rating { get; set; }
}
