// <copyright file="IEventJoinService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Defines the contract for joining events from the user interface.
/// </summary>
public interface IEventJoinService
{
    /// <summary>
    /// Attempts to enroll the current user in a specific event.
    /// </summary>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="buttonTag">The visual tag or identifier of the button that triggered the action.</param>
    /// <returns>A result object indicating success or failure with a descriptive message.</returns>
    Task<JoinEventResult> JoinEventAsync(int eventIdentifier, string buttonTag);
}