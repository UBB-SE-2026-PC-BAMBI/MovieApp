// <copyright file="ReferralValidator.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Repositories;

/// <summary>
/// Validates referral-code usage rules during enrollment.
/// </summary>
public sealed class ReferralValidator : IReferralValidator
{
    private readonly IAmbassadorRepository ambassadorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferralValidator"/> class.
    /// </summary>
    /// <param name="ambassadorRepository">The repository used for ambassador data.</param>
    public ReferralValidator(IAmbassadorRepository ambassadorRepository)
    {
        this.ambassadorRepository = ambassadorRepository;
    }

    /// <summary>
    /// Checks that a referral code exists and is not owned by the current user.
    /// </summary>
    /// <param name="referralCode">The unique referral code string.</param>
    /// <param name="currentUserIdentifier">The identifier of the user attempting to use the code.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the referral is valid; otherwise, false.</returns>
    public async Task<bool> IsValidReferralAsync(string referralCode, int currentUserIdentifier, CancellationToken cancellationToken = default)
    {
        int? ownerIdentifier = await this.ambassadorRepository.GetUserIdByReferralCodeAsync(referralCode, cancellationToken);
        if (ownerIdentifier is null)
        {
            return false;
        }

        if (ownerIdentifier.Value == currentUserIdentifier)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks code validity and ensures the user has not already used it for a specific event.
    /// </summary>
    /// <param name="referralCode">The unique referral code string.</param>
    /// <param name="currentUserIdentifier">The identifier of the user attempting to use the code.</param>
    /// <param name="eventIdentifier">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>True if the referral is valid for the specific event; otherwise, false.</returns>
    public async Task<bool> IsValidReferralForEventAsync(string referralCode, int currentUserIdentifier, int eventIdentifier, CancellationToken cancellationToken = default)
    {
        bool isBasicValid = await this.IsValidReferralAsync(referralCode, currentUserIdentifier, cancellationToken);
        if (!isBasicValid)
        {
            return false;
        }

        int? ambassadorIdentifier = await this.ambassadorRepository.GetUserIdByReferralCodeAsync(referralCode, cancellationToken);
        if (ambassadorIdentifier is null)
        {
            return false;
        }

        bool alreadyUsed = await this.ambassadorRepository.HasReferralLogAsync(ambassadorIdentifier.Value, currentUserIdentifier, eventIdentifier, cancellationToken);
        return !alreadyUsed;
    }
}
