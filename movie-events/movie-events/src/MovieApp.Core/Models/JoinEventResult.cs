// <copyright file="JoinEventResult.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Models;

/// <summary>
/// Represents the outcome of a user attempting to join a specific event.
/// </summary>
public sealed class JoinEventResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the attempt to join was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a descriptive message providing details about the success or failure.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}