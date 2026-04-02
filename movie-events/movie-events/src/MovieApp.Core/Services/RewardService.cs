using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Enforces reward redemption integrity:
/// - Blocks already-redeemed rewards.
/// - Validates event-linked reward scope before applying.
/// - Persists the redeemed state after a successful redemption.
/// </summary>
public sealed class RewardService : IRewardService
{
    private readonly IUserMovieDiscountRepository _rewardRepository;

    public RewardService(IUserMovieDiscountRepository rewardRepository)
    {
        _rewardRepository = rewardRepository;
    }

    /// <summary>
    /// Attempts to redeem a reward, optionally scoped to a specific event.
    /// Returns false if the reward is already redeemed or the event scope does not match.
    /// On success, marks the reward as redeemed in-memory and persists the state.
    /// </summary>
    public async Task<bool> RedeemAsync(Reward reward, int? eventId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reward);

        // Rule 1: Block already redeemed rewards
        if (!reward.IsAvailable)
        {
            return false;
        }

        // Rule 2: Validate event-linked reward scope
        if (reward.ApplicabilityScope == "EventSpecific")
        {
            if (reward.EventId is null || reward.EventId != eventId)
            {
                return false;
            }
        }

        // Mark redeemed in-memory and persist
        reward.Redeem();
        await _rewardRepository.MarkRedeemedAsync(reward.RewardId, cancellationToken);

        return true;
    }
}
