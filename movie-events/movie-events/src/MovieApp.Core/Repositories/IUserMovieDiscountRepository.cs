using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IUserMovieDiscountRepository
{
    Task AddAsync(Reward reward, CancellationToken cancellationToken = default);

    Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the redeemed state for a reward, so it cannot be used again.
    /// </summary>
    Task MarkRedeemedAsync(int rewardId, CancellationToken cancellationToken = default);
}

