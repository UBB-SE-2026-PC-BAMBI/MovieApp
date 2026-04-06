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
    public async Task LoadAsync_LoadsCurrentUsersReward()
    {
        var reward = new TriviaReward
        {
            Id = 5,
            UserId = 10,
            CreatedAt = DateTime.UtcNow,
            IsRedeemed = false,
        };
        var repository = new StubTriviaRewardRepository(reward);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.Same(reward, viewModel.TriviaReward);
        Assert.True(viewModel.HasTriviaReward);
        Assert.Equal("Free movie ticket — ready to use!", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_MarksRewardAsRedeemedAndPersistsIt()
    {
        var reward = new TriviaReward
        {
            Id = 5,
            UserId = 10,
            CreatedAt = DateTime.UtcNow,
            IsRedeemed = false,
        };
        var repository = new StubTriviaRewardRepository(reward);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);
        await viewModel.LoadAsync();

        await viewModel.RedeemTriviaRewardAsync();

        Assert.True(reward.IsRedeemed);
        Assert.Equal([5], repository.MarkAsRedeemedCalls);
        Assert.Equal("Already redeemed", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_DoesNothingWhenThereIsNoReward()
    {
        var repository = new StubTriviaRewardRepository(null);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.RedeemTriviaRewardAsync();

        Assert.Empty(repository.MarkAsRedeemedCalls);
    }

    // ── New Edge Case Tests ───────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_WhenNoRewardExistsForUser_HasTriviaRewardIsFalse()
    {
        var repository = new StubTriviaRewardRepository(null);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.False(viewModel.HasTriviaReward);
        Assert.Null(viewModel.TriviaReward);
    }

    [Fact]
    public async Task LoadAsync_WhenRewardAlreadyRedeemed_TriviaRewardStatusTextIsAlreadyRedeemed()
    {
        var reward = new TriviaReward
        {
            Id = 1,
            UserId = 10,
            IsRedeemed = true
        };
        var repository = new StubTriviaRewardRepository(reward);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        await viewModel.LoadAsync();

        Assert.Equal("Already redeemed", viewModel.TriviaRewardStatusText);
    }

    [Fact]
    public async Task RedeemTriviaRewardAsync_WhenRewardAlreadyRedeemed_DoesNotCallMarkRedeemed()
    {
        var reward = new TriviaReward
        {
            Id = 1,
            UserId = 10,
            IsRedeemed = true
        };
        var repository = new StubTriviaRewardRepository(reward);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);
        await viewModel.LoadAsync();

        await viewModel.RedeemTriviaRewardAsync();

        Assert.Empty(repository.MarkAsRedeemedCalls);
    }

    [Fact]
    public async Task LoadAsync_SetsIsLoadingTrueThenFalse()
    {
        var repository = new StubTriviaRewardRepository(null);
        var viewModel = new RewardsViewModel(repository, currentUserId: 10);

        var isLoadingTransitions = new List<bool>();

        viewModel.PropertyChanged += (sender, args) =>
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