// <copyright file="ReferralLogService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Repositories;

/// <summary>
/// Records successful referral-code usage and triggers reward evaluation.
/// </summary>
public sealed class ReferralLogService : IReferralLogService
{
    private readonly IAmbassadorRepository ambassadorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferralLogService"/> class.
    /// </summary>
    /// <param name="ambassadorRepository">The repository used for ambassador persistence logic.</param>
    public ReferralLogService(IAmbassadorRepository ambassadorRepository)
    {
        this.ambassadorRepository = ambassadorRepository;
    }

    /// <summary>
    /// Stores a referral interaction when the supplied code resolves to an ambassador.
    /// </summary>
    /// <param name="referralCode">The unique referral code used.</param>
    /// <param name="friendIdentifier">The user identifier of the referred user.</param>
    /// <param name="eventIdentifier">The unique identifier of the event joined.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LogReferralUsageAsync(string referralCode, int friendIdentifier, int eventIdentifier, CancellationToken cancellationToken = default)
    {
        var ambassadorId = await this.ambassadorRepository.GetUserIdByReferralCodeAsync(referralCode, cancellationToken);
        if (ambassadorId.HasValue)
        {
            await this.ambassadorRepository.AddReferralLogAsync(ambassadorId.Value, friendIdentifier, eventIdentifier, cancellationToken);
            await this.ambassadorRepository.TryApplyRewardAsync(ambassadorId.Value, cancellationToken);
        }
    }
}
