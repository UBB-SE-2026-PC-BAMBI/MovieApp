// <copyright file="IAmbassadorRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for ambassador and referral workflows.
/// </summary>
public interface IAmbassadorRepository
{
    /// <summary>
    /// Checks whether a referral code exists.
    /// </summary>
    /// <param name="referralCode">The unique alphanumeric referral code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the code is valid; otherwise, false.</returns>
    Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ambassador code owned by the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The referral code string if the user is an ambassador; otherwise, null.</returns>
    Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an ambassador profile for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user becoming an ambassador.</param>
    /// <param name="referralCode">The permanent referral code to assign.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a referral code to the owning user identifier.
    /// </summary>
    /// <param name="referralCode">The referral code to lookup.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user identifier if the code exists; otherwise, null.</returns>
    Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a referral interaction for a specific event.
    /// </summary>
    /// <param name="ambassadorId">The user identifier of the ambassador.</param>
    /// <param name="friendId">The user identifier of the referred friend.</param>
    /// <param name="eventId">The identifier of the event joined.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates whether a referral reward should be granted to an ambassador.
    /// </summary>
    /// <param name="ambassadorId">The identifier of the ambassador to evaluate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if a reward was successfully applied; otherwise, false.</returns>
    Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets referral history rows for the specified ambassador.
    /// </summary>
    /// <param name="ambassadorId">The identifier of the ambassador.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of history items documenting past referrals.</returns>
    Task<IEnumerable<ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the remaining redeemable referral reward balance for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The count of available rewards.</returns>
    Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes one referral reward from the specified user's balance.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when a referral log entry already exists for the given ambassador, friend, and event triple.
    /// </summary>
    /// <param name="ambassadorId">The identifier of the ambassador.</param>
    /// <param name="friendId">The identifier of the referred friend.</param>
    /// <param name="eventId">The identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if a log entry already exists; otherwise, false.</returns>
    Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default);
}
