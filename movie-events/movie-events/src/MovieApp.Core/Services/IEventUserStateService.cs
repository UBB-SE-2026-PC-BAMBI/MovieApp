// <copyright file="IEventUserStateService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading.Tasks;

/// <summary>
/// Provides logic for determining the current user's state relative to specific events.
/// </summary>
public interface IEventUserStateService
{
    /// <summary>
    /// Calculates the available discount for a user for a specific event.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <returns>The calculated discount value as an integer.</returns>
    Task<int> GetDiscountForEventAsync(int eventIdentifier);

    /// <summary>
    /// Checks if the current user has already joined the specified event.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <returns>True if the user is enrolled; otherwise, false.</returns>
    Task<bool> IsEventJoinedByUserAsync(int eventIdentifier);
}