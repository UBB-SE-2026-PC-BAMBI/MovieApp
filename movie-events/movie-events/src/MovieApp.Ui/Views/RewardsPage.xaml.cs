// <copyright file="RewardsPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels;

/// <summary>
/// Represents the rewards page, displaying trivia rewards, slot rewards,
/// and allowing users to redeem available rewards.
/// </summary>
public sealed partial class RewardsPage : Page
{
    private RewardsViewModel? viewModel;

    private int rewardBalance;
    private List<SlotRewardItem>? loadedSlotRewards;
    private bool slotsListPopulated;

    /// <summary>
    /// Initializes a new instance of the <see cref="RewardsPage"/> class.
    /// </summary>
    public RewardsPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the reward balance of the current user.
    /// </summary>
    public int RewardBalance
    {
        get => this.rewardBalance;
        private set
        {
            if (this.rewardBalance != value)
            {
                this.rewardBalance = value;
                this.Bindings.Update();
            }
        }
    }

    /// <summary>
    /// Called when the page is navigated to and loads reward data for the current user.
    /// </summary>
    /// <param name="e">The navigation event data.</param>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        User? currentUser = App.Services.CurrentUserService?.CurrentUser;

        if (currentUser is not null)
        {
            if (App.Services.AmbassadorRepository is not null)
            {
                this.RewardBalance =
                    await App.Services.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
            }

            await this.LoadSlotRewardsAsync(currentUser.Id);
        }

        if (App.Services.TriviaRewardRepository is not null)
        {
            this.viewModel = new RewardsViewModel(App.Services.TriviaRewardRepository, App.CurrentUserId);
            await this.viewModel.LoadAsync();
            this.UpdateTriviaRewardUI();
        }
    }

    private void UpdateTriviaRewardUI()
    {
        if (this.viewModel is null)
        {
            return;
        }

        if (this.viewModel.TriviaReward is null)
        {
            this.NoRewardBanner.Visibility = Visibility.Visible;
            this.RewardAvailableBanner.Visibility = Visibility.Collapsed;
            this.RedeemedBanner.Visibility = Visibility.Collapsed;
        }
        else if (this.viewModel.TriviaReward.IsRedeemed)
        {
            this.NoRewardBanner.Visibility = Visibility.Collapsed;
            this.RewardAvailableBanner.Visibility = Visibility.Collapsed;
            this.RedeemedBanner.Visibility = Visibility.Visible;
        }
        else
        {
            this.NoRewardBanner.Visibility = Visibility.Collapsed;
            this.RewardAvailableBanner.Visibility = Visibility.Visible;
            this.RedeemedBanner.Visibility = Visibility.Collapsed;
            this.RewardEarnedDateText.Text =
                $"Earned on {this.viewModel.TriviaReward.CreatedAt:dd MMM yyyy}";
        }
    }

    private async Task LoadSlotRewardsAsync(int userId)
    {
        if (App.Services.UserMovieDiscountRepository is null)
        {
            return;
        }

        IEnumerable<Reward> rewards =
            await App.Services.UserMovieDiscountRepository.GetDiscountsForUserAsync(userId);

        this.loadedSlotRewards = rewards
            .Select(r => new SlotRewardItem
            {
                RewardId = r.RewardId,
                MovieTitle = r.ApplicabilityScope,
                DiscountText = $"{(int)r.DiscountValue}% off",
                IsRedeemed = r.RedemptionStatus,
            })
            .ToList();

        this.ApplySlotRewardsToListView();
    }

    private void ApplySlotRewardsToListView()
    {
        if (this.loadedSlotRewards is null || this.SlotsListView is null)
        {
            return;
        }

        this.SlotsListView.ItemsSource = this.loadedSlotRewards;
        this.slotsListPopulated = true;
    }

    private void RewardsTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!this.slotsListPopulated && this.loadedSlotRewards is not null
            && (TabViewItem)this.RewardsTabView.SelectedItem == this.SlotsTab)
        {
            this.ApplySlotRewardsToListView();
        }
    }

    private void SlotsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.SlotsListView.SelectedItem is not SlotRewardItem item)
        {
            this.ClearDetailPanel();
            return;
        }

        this.DetailTypeBox.Text = "Jackpot Discount";
        this.DetailScopeBox.Text = item.MovieTitle;
        this.DetailDiscountBox.Text = item.DiscountText;
        this.DetailStatusBox.Text = item.IsRedeemed ? "Redeemed" : "Available";
        this.RedeemButton.IsEnabled = !item.IsRedeemed;
    }

    private void ClearDetailPanel()
    {
        this.DetailTypeBox.Text = string.Empty;
        this.DetailScopeBox.Text = string.Empty;
        this.DetailDiscountBox.Text = string.Empty;
        this.DetailStatusBox.Text = string.Empty;
        this.RedeemButton.IsEnabled = false;
    }

    private async void RedeemButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.SlotsListView.SelectedItem is not SlotRewardItem item || item.IsRedeemed)
        {
            return;
        }

        if (App.Services.UserMovieDiscountRepository is null
            || App.Services.CurrentUserService?.CurrentUser is null)
        {
            return;
        }

        await App.Services.UserMovieDiscountRepository.MarkRedeemedAsync(item.RewardId);

        item.IsRedeemed = true;
        this.DetailStatusBox.Text = "Redeemed";
        this.RedeemButton.IsEnabled = false;

        await this.LoadSlotRewardsAsync(App.Services.CurrentUserService.CurrentUser.Id);
    }

    private async void TriviaRedeemButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.viewModel is null)
        {
            return;
        }

        this.TriviaRedeemButton.IsEnabled = false;
        await this.viewModel.RedeemTriviaRewardAsync();
        this.UpdateTriviaRewardUI();
    }
}

/// <summary>
/// Represents a slot reward item displayed in the rewards list.
/// </summary>
public sealed class SlotRewardItem
{
    /// <summary>
    /// Gets or sets the reward identifier.
    /// </summary>
    public int RewardId { get; set; }

    /// <summary>
    /// Gets or sets the movie title associated with the reward.
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the discount text for the reward.
    /// </summary>
    public string DiscountText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the reward has been redeemed.
    /// </summary>
    public bool IsRedeemed { get; set; }

    /// <summary>
    /// Gets the status label for the reward.
    /// </summary>
    public string StatusLabel => this.IsRedeemed ? "Redeemed" : this.DiscountText;

    /// <summary>
    /// Gets the brush representing the reward status.
    /// </summary>
    public SolidColorBrush StatusBrush => new SolidColorBrush(
        this.IsRedeemed
            ? Windows.UI.Color.FromArgb(0x33, 0x94, 0x94, 0x94)
            : Windows.UI.Color.FromArgb(0x33, 0x16, 0xA3, 0x4A));
}