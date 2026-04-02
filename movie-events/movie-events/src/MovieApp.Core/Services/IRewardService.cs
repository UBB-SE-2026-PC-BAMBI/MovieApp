using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Enforces reward redemption integrity rules.
/// </summary>
public interface IRewardService
{
    /// <summary>
    /// Attempts to redeem a reward, optionally scoped to a specific event.
    /// Returns false if the reward is already redeemed or the event scope does not match.
    /// On success, marks the reward as redeemed and persists the state.
    /// </summary>
    Task<bool> RedeemAsync(Reward reward, int? eventId, CancellationToken cancellationToken = default);
}
