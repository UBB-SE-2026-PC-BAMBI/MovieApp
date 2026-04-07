// <copyright file="ITriviaRewardRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for managing trivia-wheel rewards.
/// </summary>
public interface ITriviaRewardRepository
{
    /// <summary>
    /// Adds a new trivia reward to the persistence store.
    /// </summary>
    /// <param name="reward">The trivia reward entity to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the oldest unredeemed reward for a specific user.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The unredeemed trivia reward if found; otherwise, null.</returns>
    Task<TriviaReward?> GetUnredeemedByUserAsync(int userIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a specific trivia reward as successfully redeemed.
    /// </summary>
    /// <param name="rewardIdentifier">The unique identifier of the reward.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkAsRedeemedAsync(int rewardIdentifier, CancellationToken cancellationToken = default);
}