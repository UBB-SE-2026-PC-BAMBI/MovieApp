using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class RewardServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Reward MakeReward(string scope = "Global", int? eventId = null, bool redeemed = false) =>
        new()
        {
            RewardId = 1,
            RewardType = "Discount",
            OwnerUserId = 10,
            ApplicabilityScope = scope,
            DiscountValue = 15,
            EventId = eventId,
            RedemptionStatus = redeemed,
        };

    // ── Block already redeemed rewards ────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_RewardIsAlreadyRedeemed_ReturnsFalse()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward(redeemed: true);

        bool result = await service.RedeemAsync(reward, eventIdentifier: null);

        Assert.False(result);
        Assert.Empty(repo.MarkedRedeemedIds);
    }

    // ── Event-scope validation ────────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_EventScopeMismatch_ReturnsFalse()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward(scope: "EventSpecific", eventId: 5);

        // Trying to redeem against event 99 — wrong event
        bool result = await service.RedeemAsync(reward, eventIdentifier: 99);

        Assert.False(result);
        Assert.Empty(repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_EventScopeWithoutEventId_ReturnsFalse()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward(scope: "EventSpecific", eventId: 5);

        bool result = await service.RedeemAsync(reward, eventIdentifier: null);

        Assert.False(result);
        Assert.Empty(repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_EventScopeMatches_ReturnsTrue()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward(scope: "EventSpecific", eventId: 5);

        bool result = await service.RedeemAsync(reward, eventIdentifier: 5);

        Assert.True(result);
        Assert.Contains(1, repo.MarkedRedeemedIds);
        Assert.True(reward.RedemptionStatus);
    }

    // ── Global rewards ────────────────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_GlobalScope_ReturnsTrue()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward(scope: "Global");

        bool result = await service.RedeemAsync(reward, eventIdentifier: null);

        Assert.True(result);
        Assert.Contains(1, repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_ValidReward_MarksRewardAsRedeemed()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward();

        await service.RedeemAsync(reward, eventIdentifier: null);

        Assert.Contains(reward.RewardId, repo.MarkedRedeemedIds);
    }

    // ── Stacked reward scenarios ──────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_CalledTwiceOnSameReward_SecondCallReturnsFalse()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);
        Reward reward = MakeReward();

        bool first = await service.RedeemAsync(reward, eventIdentifier: null);
        bool second = await service.RedeemAsync(reward, eventIdentifier: null);

        Assert.True(first);
        Assert.False(second);
        // MarkRedeemed should only be called once (on the successful first call)
        Assert.Single(repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_TwoDistinctValidRewards_BothCallsReturnTrue()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);

        Reward reward1 = MakeReward();
        Reward reward2 = new Reward
        {
            RewardId = 2,
            RewardType = "Discount",
            OwnerUserId = 10,
            ApplicabilityScope = "Global",
            DiscountValue = 10,
        };

        bool first = await service.RedeemAsync(reward1, eventIdentifier: null);
        bool second = await service.RedeemAsync(reward2, eventIdentifier: null);

        Assert.True(first);
        Assert.True(second);
        Assert.Equal(2, repo.MarkedRedeemedIds.Count);
    }

    [Fact]
    public async Task RedeemAsync_MixedScopeRewards_OnlyValidRewardsSucceed()
    {
        StubRewardRepository repo = new StubRewardRepository();
        RewardService service = new RewardService(repo);

        Reward globalReward = MakeReward(scope: "Global");
        Reward eventReward = new Reward
        {
            RewardId = 2,
            RewardType = "Discount",
            OwnerUserId = 10,
            ApplicabilityScope = "EventSpecific",
            DiscountValue = 20,
            EventId = 5,
        };

        bool globalOk = await service.RedeemAsync(globalReward, eventIdentifier: 5);
        bool eventOk = await service.RedeemAsync(eventReward, eventIdentifier: 5);
        bool eventFail = await service.RedeemAsync(
            new Reward { RewardId = 3, RewardType = "Discount", OwnerUserId = 10, ApplicabilityScope = "EventSpecific", DiscountValue = 20, EventId = 5 },
            eventIdentifier: 99);

        Assert.True(globalOk);
        Assert.True(eventOk);
        Assert.False(eventFail);
    }

    // ── Stub ──────────────────────────────────────────────────────────────────

    private sealed class StubRewardRepository : IUserMovieDiscountRepository
    {
        public List<int> MarkedRedeemedIds { get; } = [];

        public Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<Reward>());

        public Task MarkRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
        {
            MarkedRedeemedIds.Add(rewardId);
            return Task.CompletedTask;
        }
    }
}
