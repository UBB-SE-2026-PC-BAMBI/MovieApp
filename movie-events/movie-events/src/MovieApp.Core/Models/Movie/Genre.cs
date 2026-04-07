// <copyright file="Genre.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models.Movie;

/// <summary>
/// Represents a category or classification of movie content.
/// </summary>
public sealed class Genre
{
    /// <summary>
    /// Gets or sets the unique identifier for the genre.
    /// </summary>
    required public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the genre.
    /// </summary>
    required public string Name { get; set; } = string.Empty;
}