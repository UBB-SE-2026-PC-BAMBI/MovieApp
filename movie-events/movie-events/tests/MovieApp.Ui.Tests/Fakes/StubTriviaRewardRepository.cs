using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.Tests.Fakes;

public sealed class StubTriviaRewardRepository : ITriviaRewardRepository
{
    private readonly TriviaReward? _reward;

    public List<int> MarkAsRedeemedCalls { get; } = new();

    public StubTriviaRewardRepository(TriviaReward? reward)
    {
        _reward = reward;
    }

    public Task AddAsync(TriviaReward newReward, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_reward);
    }

    public Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        MarkAsRedeemedCalls.Add(rewardId);
        return Task.CompletedTask;
    }
}