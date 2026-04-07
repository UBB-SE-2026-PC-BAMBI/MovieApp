// <copyright file="IUserEventAttendanceRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides persistence operations for tracking user event attendance and registration.
/// </summary>
public interface IUserEventAttendanceRepository
{
    /// <summary>
    /// Retrieves the identifiers of all events a specific user has joined.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of event identifiers.</returns>
    Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a user's attendance or registration for a specific event.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task JoinAsync(int userIdentifier, int eventIdentifier, CancellationToken cancellationToken = default);
}
