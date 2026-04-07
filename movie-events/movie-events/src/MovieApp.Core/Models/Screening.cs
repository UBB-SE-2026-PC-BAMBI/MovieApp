// <copyright file="Screening.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents a screening that maps a movie to an event with a specific screening time.
/// </summary>
public sealed class Screening
{
    /// <summary>
    /// Gets the unique identifier for the screening record.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the associated event.
    /// </summary>
    required public int EventId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the movie being screened.
    /// </summary>
    required public int MovieId { get; init; }

    /// <summary>
    /// Gets the specific date and time the screening takes place.
    /// </summary>
    required public DateTime ScreeningTime { get; init; }
}
