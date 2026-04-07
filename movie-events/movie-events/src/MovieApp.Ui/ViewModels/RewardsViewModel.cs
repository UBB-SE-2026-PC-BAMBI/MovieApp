// <copyright file="RewardsViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Exposes the current user's trivia reward state to the UI.
/// </summary>
public sealed class RewardsViewModel : ViewModelBase
{
    private readonly ITriviaRewardRepository triviaRewardRepository;
    private readonly int currentUserId;

    private TriviaReward? triviaReward;
    private bool isLoading;

    /// <summary>
    /// Initializes a new instance of the <see cref="RewardsViewModel"/> class.
    /// Creates the rewards view model for the supplied user.
    /// </summary>
    /// <param name="triviaRewardRepository">The repository used to manage trivia rewards.</param>
    /// <param name="currentUserId">The identifier of the current user.</param>
    public RewardsViewModel(ITriviaRewardRepository triviaRewardRepository, int currentUserId)
    {
        this.triviaRewardRepository = triviaRewardRepository;
        this.currentUserId = currentUserId;
    }

    /// <summary>
    /// Gets the current trivia reward for the user.
    /// </summary>
    public TriviaReward? TriviaReward
    {
        get => this.triviaReward;
        private set
        {
            this.triviaReward = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.HasTriviaReward));
            this.OnPropertyChanged(nameof(this.TriviaRewardStatusText));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the reward data is currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set
        {
            this.isLoading = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a trivia reward is available.
    /// </summary>
    public bool HasTriviaReward => this.TriviaReward is not null;

    /// <summary>
    /// Gets the display text describing the current trivia reward state.
    /// </summary>
    public string TriviaRewardStatusText => this.TriviaReward is null
        ? "No reward available"
        : this.TriviaReward.IsRedeemed
            ? "Already redeemed"
            : "Free movie ticket — ready to use!";

    /// <summary>
    /// Loads the current user's unredeemed trivia reward.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoadAsync()
    {
        this.IsLoading = true;
        this.TriviaReward =
            await this.triviaRewardRepository.GetUnredeemedByUserAsync(this.currentUserId);
        this.IsLoading = false;
    }

    /// <summary>
    /// Redeems the loaded trivia reward and persists the redemption state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RedeemTriviaRewardAsync()
    {
        if (this.TriviaReward is null || this.TriviaReward.IsRedeemed)
        {
            return;
        }

        this.TriviaReward.Redeem();
        await this.triviaRewardRepository.MarkAsRedeemedAsync(this.TriviaReward.Id);
        this.OnPropertyChanged(nameof(this.TriviaRewardStatusText));
        this.OnPropertyChanged(nameof(this.TriviaReward));
    }
}
