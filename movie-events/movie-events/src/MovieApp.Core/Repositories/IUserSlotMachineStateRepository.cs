// <copyright file="IUserSlotMachineStateRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for user spin data and slot machine state.
/// </summary>
public interface IUserSlotMachineStateRepository
{
    /// <summary>
    /// Retrieves the current spin data and login state for a specific user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user's spin data if it exists; otherwise, null.</returns>
    Task<UserSpinData?> GetByUserIdAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an initial spin data record for a new user.
    /// </summary>
    /// <param name="userSpinData">The initial user spin data state.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateAsync(UserSpinData userSpinData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing spin data record with new values.
    /// </summary>
    /// <param name="userSpinData">The updated user spin data state.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(UserSpinData userSpinData, CancellationToken cancellationToken = default);
}
