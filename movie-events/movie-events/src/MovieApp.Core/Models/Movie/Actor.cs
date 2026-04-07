// <copyright file="Actor.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models.Movie;

using System;

/// <summary>
/// Class for actor.
/// </summary>
public sealed class Actor
{
    /// <summary>
    /// Gets or sets Id for Actor.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets Name for Actor.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
