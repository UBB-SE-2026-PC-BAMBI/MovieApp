// <copyright file="SlotMachinePage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Models;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using Windows.UI;

/// <summary>
/// Represents the slot machine page, allowing users to spin for rewards
/// and view event details based on outcomes.
/// </summary>
public sealed partial class SlotMachinePage : Page
{
    private readonly EventDialogContentBuilder dialogBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotMachinePage"/> class.
    /// </summary>
    public SlotMachinePage()
    {
        this.dialogBuilder = new EventDialogContentBuilder(
            App.Services.ReferralValidator,
            App.Services.CurrentUserService);

        this.InitializeComponent();
        this.Loaded += this.OnPageLoaded;
    }

    /// <summary>
    /// Handles the page loaded event and initializes the slot machine view model.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnPageLoaded;

        User? currentUser = App.Services.CurrentUserService?.CurrentUser;
        if (currentUser is null)
        {
            return;
        }

        SlotMachineViewModel viewModel = new SlotMachineViewModel(
            currentUser.Id,
            App.Services.SlotMachineService ?? throw new System.InvalidOperationException("SlotMachineService not initialized"),
            App.Services.SlotMachineAnimationService ?? throw new System.InvalidOperationException("SlotMachineAnimationService not initialized"));

        viewModel.JackpotHit += this.OnJackpotHit;
        this.DataContext = viewModel;
        await viewModel.InitializeAsync();
        this.Focus(FocusState.Programmatic);
    }

    private async void OnJackpotHit(Movie movie, int discountPercentage)
    {
        ContentDialog dialog = (ContentDialog)this.Resources["JackpotContentDialog"];
        dialog.XamlRoot = this.XamlRoot;

        JackpotDialogViewModel jackpotViewModel = new JackpotDialogViewModel(movie, discountPercentage);

        dialog.Content = jackpotViewModel;
        await dialog.ShowAsync();

        this.Focus(FocusState.Programmatic);
    }

    private async void OpenEventDetailsDialog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not MatchingEventItem item)
        {
            return;
        }

        Event selectedEvent = item.Event;

        int discountPercentage = 0;

        if (item.IsJackpotEvent)
        {
            if (App.Services.EventUserStateService is not null)
            {
                int percentage = await App.Services.EventUserStateService.GetDiscountForEventAsync(selectedEvent.Id);
                discountPercentage = percentage > 0 ? percentage : 70;
            }
            else
            {
                discountPercentage = 70;
            }
        }

        ContentDialog dialog = await this.dialogBuilder.BuildDialogAsync(
            selectedEvent,
            this.XamlRoot,
            isJackpotEvent: item.IsJackpotEvent,
            discountPercentage: item.IsJackpotEvent ? discountPercentage : null);

        await dialog.ShowAsync();

        this.Focus(FocusState.Programmatic);

        if (this.DataContext is SlotMachineViewModel slotMachineViewModel)
        {
            await slotMachineViewModel.RefreshSpinCountAsync();
        }
    }
}