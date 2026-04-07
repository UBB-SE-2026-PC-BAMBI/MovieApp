// <copyright file="IReferralLogService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Manages the logging of referral code usage.
/// </summary>
public interface IReferralLogService
{
    /// <summary>
    /// Records the successful usage of a referral code by a friend.
    /// </summary>
    /// <param name="referralCode">The code being used.</param>
    /// <param name="friendIdentifier">The user identifier of the referred friend.</param>
    /// <param name="eventIdentifier">The identifier of the event being joined.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogReferralUsageAsync(string referralCode, int friendIdentifier, int eventIdentifier, CancellationToken cancellationToken = default);
}