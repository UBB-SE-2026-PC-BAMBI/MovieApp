// <copyright file="IRewardService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Enforces reward redemption integrity rules.
/// </summary>
public interface IRewardService
{
    /// <summary>
    /// Attempts to redeem a reward, optionally scoped to a specific event.
    /// </summary>
    /// <param name="reward">The reward entity to redeem.</param>
    /// <param name="eventIdentifier">The optional identifier of the event scope.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the reward was successfully marked as redeemed; otherwise, false.</returns>
    Task<bool> RedeemAsync(Reward reward, int? eventIdentifier, CancellationToken cancellationToken = default);
}
