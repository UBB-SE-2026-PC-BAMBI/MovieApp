// <copyright file="RewardService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Enforces reward redemption integrity rules.
/// </summary>
public sealed class RewardService : IRewardService
{
    private readonly IUserMovieDiscountRepository rewardRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RewardService"/> class.
    /// </summary>
    /// <param name="rewardRepository">The repository for reward persistence.</param>
    public RewardService(IUserMovieDiscountRepository rewardRepository)
    {
        this.rewardRepository = rewardRepository;
    }

    /// <summary>
    /// Attempts to redeem a reward, optionally scoped to a specific event.
    /// </summary>
    /// <param name="reward">The reward entity to redeem.</param>
    /// <param name="eventIdentifier">The optional identifier of the event scope.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the reward was successfully marked as redeemed; otherwise, false.</returns>
    public async Task<bool> RedeemAsync(Reward reward, int? eventIdentifier, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reward);

        if (!reward.IsAvailable)
        {
            return false;
        }

        if (reward.ApplicabilityScope == "EventSpecific")
        {
            if (reward.EventId is null || reward.EventId != eventIdentifier)
            {
                return false;
            }
        }

        reward.Redeem();
        await this.rewardRepository.MarkRedeemedAsync(reward.RewardId, cancellationToken);

        return true;
    }
}
