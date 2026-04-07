// <copyright file="IReferralValidator.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Validates referral codes against system business rules.
/// </summary>
public interface IReferralValidator
{
    /// <summary>
    /// Validates that a referral code exists and is not owned by the current user.
    /// </summary>
    /// <param name="referralCode">The code to validate.</param>
    /// <param name="currentUserIdentifier">The identifier of the user attempting to use the code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the referral is valid; otherwise, false.</returns>
    Task<bool> IsValidReferralAsync(string referralCode, int currentUserIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks code validity and ensures the user has not already used it for a specific event.
    /// </summary>
    /// <param name="referralCode">The code to validate.</param>
    /// <param name="currentUserIdentifier">The identifier of the user attempting to use the code.</param>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the referral is valid for the specific event; otherwise, false.</returns>
    Task<bool> IsValidReferralForEventAsync(string referralCode, int currentUserIdentifier, int eventIdentifier, CancellationToken cancellationToken = default);
}