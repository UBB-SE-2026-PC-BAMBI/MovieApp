using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Ui.Tests.Fakes;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class RewardsViewModelTests
{
    [Fact]
    public async Task LoadAsync_RewardExistsForUser_SetsTriviaRewardAndStatusText()
    {
        TriviaReward reward = new TriviaReward
        {
            Id = 5,
            UserId = 10,
            CreatedAt = DateTime.UtcNow,
            IsRedeemed = false,
        };
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(reward);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.Same(reward, viewModel.TriviaReward);
        Assert.True(viewModel.HasTriviaReward);
        Assert.Equal("Free movie ticket — ready to use!", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_UnredeemedRewardExists_MarksRedeemedAndCallsRepository()
    {
        TriviaReward reward = new TriviaReward
        {
            Id = 5,
            UserId = 10,
            CreatedAt = DateTime.UtcNow,
            IsRedeemed = false,
        };
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(reward);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);
        await viewModel.LoadAsync();

        await viewModel.RedeemTriviaRewardAsync();

        Assert.True(reward.IsRedeemed);
        Assert.Equal([5], repository.MarkAsRedeemedCalls);
        Assert.Equal("Already redeemed", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_NoRewardExists_DoesNotCallRepository()
    {
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(null);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.RedeemTriviaRewardAsync();

        Assert.Empty(repository.MarkAsRedeemedCalls);
    }

    // ── New Edge Case Tests ───────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_NoRewardExists_SetsHasTriviaRewardToFalse()
    {
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(null);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.False(viewModel.HasTriviaReward);
        Assert.Null(viewModel.TriviaReward);
    }

    [Fact]
    public async Task LoadAsync_RewardAlreadyRedeemed_SetsStatusTextToAlreadyRedeemed()
    {
        TriviaReward reward = new TriviaReward
        {
            Id = 1,
            UserId = 10,
            IsRedeemed = true
        };
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(reward);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.Equal("Already redeemed", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_RewardAlreadyRedeemed_DoesNotCallRepository()
    {
        TriviaReward reward = new TriviaReward
        {
            Id = 1,
            UserId = 10,
            IsRedeemed = true
        };
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(reward);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);
        await viewModel.LoadAsync();

        await viewModel.RedeemTriviaRewardAsync();

        Assert.Empty(repository.MarkAsRedeemedCalls);
    }

    [Fact]
    public async Task LoadAsync_WhenCalled_TogglesIsLoadingProperty()
    {
        StubTriviaRewardRepository repository = new StubTriviaRewardRepository(null);
        RewardsViewModel viewModel = new RewardsViewModel(repository, currentUserId: 10);

        List<bool> isLoadingTransitions = new List<bool>();

        viewModel.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs args) =>
        {
            if (args.PropertyName == nameof(viewModel.IsLoading))
            {
                isLoadingTransitions.Add(viewModel.IsLoading);
            }
        };

        await viewModel.LoadAsync();

        Assert.Equal(2, isLoadingTransitions.Count);
        Assert.True(isLoadingTransitions[0]);
        Assert.False(isLoadingTransitions[1]);
    }
}