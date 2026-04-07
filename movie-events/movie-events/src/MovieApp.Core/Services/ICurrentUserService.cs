// <copyright file="ICurrentUserService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using MovieApp.Core.Models;

/// <summary>
/// Exposes the currently authenticated application user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the initialized current user.
    /// </summary>
    User CurrentUser { get; }

    /// <summary>
    /// Initializes the current-user context.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of favorite event links.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
