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
    public async Task RedeemAsync_ReturnsFalse_WhenAlreadyRedeemed()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward(redeemed: true);

        var result = await service.RedeemAsync(reward, eventId: null);

        Assert.False(result);
        Assert.Empty(repo.MarkedRedeemedIds);
    }

    // ── Event-scope validation ────────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_ReturnsFalse_WhenEventScopeMismatch()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward(scope: "EventSpecific", eventId: 5);

        // Trying to redeem against event 99 — wrong event
        var result = await service.RedeemAsync(reward, eventId: 99);

        Assert.False(result);
        Assert.Empty(repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_ReturnsFalse_WhenEventScopeButNoEventProvided()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward(scope: "EventSpecific", eventId: 5);

        var result = await service.RedeemAsync(reward, eventId: null);

        Assert.False(result);
        Assert.Empty(repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_ReturnsTrue_WhenEventScopeMatches()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward(scope: "EventSpecific", eventId: 5);

        var result = await service.RedeemAsync(reward, eventId: 5);

        Assert.True(result);
        Assert.Contains(1, repo.MarkedRedeemedIds);
        Assert.True(reward.RedemptionStatus);
    }

    // ── Global rewards ────────────────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_ReturnsTrue_WhenGlobalScope_NoEventRequired()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward(scope: "Global");

        var result = await service.RedeemAsync(reward, eventId: null);

        Assert.True(result);
        Assert.Contains(1, repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_CallsMarkRedeemed_OnSuccess()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward();

        await service.RedeemAsync(reward, eventId: null);

        Assert.Contains(reward.RewardId, repo.MarkedRedeemedIds);
    }

    // ── Stacked reward scenarios ──────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_Stacked_SecondCallFails_AfterFirstSucceeds()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);
        var reward = MakeReward();

        var first = await service.RedeemAsync(reward, eventId: null);
        var second = await service.RedeemAsync(reward, eventId: null);

        Assert.True(first);
        Assert.False(second);
        // MarkRedeemed should only be called once (on the successful first call)
        Assert.Single(repo.MarkedRedeemedIds);
    }

    [Fact]
    public async Task RedeemAsync_Stacked_TwoDistinctRewards_BothSucceed()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);

        var reward1 = MakeReward();
        var reward2 = new Reward
        {
            RewardId = 2,
            RewardType = "Discount",
            OwnerUserId = 10,
            ApplicabilityScope = "Global",
            DiscountValue = 10,
        };

        var first = await service.RedeemAsync(reward1, eventId: null);
        var second = await service.RedeemAsync(reward2, eventId: null);

        Assert.True(first);
        Assert.True(second);
        Assert.Equal(2, repo.MarkedRedeemedIds.Count);
    }

    [Fact]
    public async Task RedeemAsync_Stacked_MixedScope_OnlyMatchingEventSucceeds()
    {
        var repo = new StubRewardRepository();
        var service = new RewardService(repo);

        var globalReward = MakeReward(scope: "Global");
        var eventReward = new Reward
        {
            RewardId = 2,
            RewardType = "Discount",
            OwnerUserId = 10,
            ApplicabilityScope = "EventSpecific",
            DiscountValue = 20,
            EventId = 5,
        };

        var globalOk = await service.RedeemAsync(globalReward, eventId: 5);
        var eventOk = await service.RedeemAsync(eventReward, eventId: 5);
        var eventFail = await service.RedeemAsync(
            new Reward { RewardId = 3, RewardType = "Discount", OwnerUserId = 10, ApplicabilityScope = "EventSpecific", DiscountValue = 20, EventId = 5 },
            eventId: 99);

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
