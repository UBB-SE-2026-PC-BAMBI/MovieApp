// <copyright file="Marathon.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Represents a themed movie marathon event with specific scoping and requirements.
/// </summary>
public sealed class Marathon
{
    /// <summary>
    /// Gets the unique identifier for the marathon.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the title of the marathon.
    /// </summary>
    required public string Title { get; init; }

    /// <summary>
    /// Gets or sets the descriptive text for the marathon.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL for the marathon's promotional poster.
    /// </summary>
    public string? PosterUrl { get; set; }

    /// <summary>
    /// Gets or sets the visual or content theme of the marathon.
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the marathon that must be completed first.
    /// </summary>
    public int? PrerequisiteMarathonId { get; set; }

    /// <summary>
    /// Gets or sets the date this marathon was last highlighted.
    /// </summary>
    public DateTime? LastFeaturedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the marathon is currently available.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the temporal scope (e.g., a specific week string).
    /// </summary>
    public string? WeekScoping { get; set; }

    /// <summary>
    /// Gets a value indicating whether this marathon requires a prerequisite completion.
    /// </summary>
    public bool IsElite => this.PrerequisiteMarathonId.HasValue;
}