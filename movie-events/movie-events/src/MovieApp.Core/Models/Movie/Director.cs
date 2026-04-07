// <copyright file="Director.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models.Movie;

/// <summary>
/// Represents a director involved in movie production.
/// </summary>
public sealed class Director
{
    /// <summary>
    /// Gets or sets the unique identifier for the director.
    /// </summary>
    required public int Id { get; set; }

    /// <summary>
    /// Gets or sets the full name of the director.
    /// </summary>
    required public string Name { get; set; } = string.Empty;
}