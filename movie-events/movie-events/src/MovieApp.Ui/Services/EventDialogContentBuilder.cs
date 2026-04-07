// <copyright file="EventDialogContentBuilder.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Views;

/// <summary>
/// Builds <see cref="ContentDialog"/> instances for displaying event details.
/// </summary>
public sealed class EventDialogContentBuilder
{
    private readonly IReferralValidator? referralValidator;
    private readonly ICurrentUserService? currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDialogContentBuilder"/> class.
    /// </summary>
    /// <param name="referralValidator">The referral validator service.</param>
    /// <param name="currentUserService">The current user service.</param>
    public EventDialogContentBuilder(IReferralValidator? referralValidator, ICurrentUserService? currentUserService)
    {
        this.referralValidator = referralValidator;
        this.currentUserService = currentUserService;
    }

    /// <summary>
    /// Builds a <see cref="ContentDialog"/> for the specified event.
    /// </summary>
    /// <param name="movieEvent">The event to display.</param>
    /// <param name="xamlRoot">The XAML root used for dialog placement.</param>
    /// <param name="isJackpotEvent">Indicates whether the event is a jackpot event.</param>
    /// <param name="discountPercentage">The optional discount percentage applied to the event.</param>
    /// <returns>A task that represents the asynchronous operation, containing the constructed dialog.</returns>
    public async Task<ContentDialog> BuildDialogAsync(
        Event movieEvent,
        XamlRoot xamlRoot,
        bool isJackpotEvent = false,
        int? discountPercentage = null)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = movieEvent.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
        };

        EventDialogViewModel dialogViewModel = await this.BuildViewModelAsync(
            movieEvent,
            isJackpotEvent,
            discountPercentage,
            () => dialog.Hide(),
            xamlRoot);

        dialog.Content = EventDialogViewBuilder.Create(dialogViewModel);

        return dialog;
    }

    /// <summary>
    /// Builds the <see cref="EventDialogViewModel"/> used by the dialog.
    /// </summary>
    /// <param name="movieEvent">The event to display.</param>
    /// <param name="isJackpotEvent">Indicates whether the event is a jackpot event.</param>
    /// <param name="discountPercent">The optional discount percentage.</param>
    /// <param name="closeDialogAction">The action used to close the dialog.</param>
    /// <param name="xamlRoot">The XAML root used for dialog placement.</param>
    /// <returns>A task that represents the asynchronous operation, containing the view model.</returns>
    private async Task<EventDialogViewModel> BuildViewModelAsync(
        Event movieEvent,
        bool isJackpotEvent,
        int? discountPercent,
        Action closeDialogAction,
        XamlRoot xamlRoot)
    {
        EventDialogViewModel model = new EventDialogViewModel(movieEvent, isJackpotEvent, discountPercent)
        {
            Description = movieEvent.Description ?? "A curated movie experience with limited seating.",
            FormattedDate = $"When: {movieEvent.EventDateTime:g}",
            Location = $"Where: {movieEvent.LocationReference}",
            PriceText = $"Price: {EventCard.GetPriceText(movieEvent, CultureInfo.CurrentCulture)}",
            RatingText = $"Rating: {EventCard.GetRatingText(movieEvent)}",
            CapacityText = $"Seats: {EventCard.GetCapacityText(movieEvent)}",
        };

        if (isJackpotEvent && discountPercent.HasValue)
        {
            model.ShowJackpotBanner = true;
            model.JackpotBannerText = $"{discountPercent.Value}% Jackpot Discount applies to this event!";
            model.JackpotDiscountedPriceText = $"Discounted price: {EventCard.GetDiscountedPriceText(movieEvent, CultureInfo.CurrentCulture, discountPercent.Value)}";
        }
        else if (!isJackpotEvent && discountPercent > 0 && movieEvent.TicketPrice > 0)
        {
            model.ShowRegularDiscountBanner = true;
            model.RegularDiscountedPriceText = $"Your price: {EventCard.GetDiscountedPriceText(movieEvent, CultureInfo.CurrentCulture, discountPercent.Value)}";
        }

        User? currentUser = this.currentUserService?.CurrentUser;

        if (App.Services.AmbassadorRepository is not null && currentUser is not null)
        {
            int balance = await App.Services.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
            if (balance > 0)
            {
                model.HasFreePass = true;
                model.FreePassBalance = balance;
            }
        }

        model.ValidateReferralAction = async (string code) =>
        {
            if (string.IsNullOrWhiteSpace(code) || this.referralValidator is null || currentUser is null)
            {
                return false;
            }

            return await this.referralValidator.IsValidReferralAsync(code, currentUser.Id);
        };

        model.UseFreePassAction = async () =>
        {
            closeDialogAction();

            ContentDialog confirmDialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = "Use Free Pass?",
                Content = "This will use 1 free enrollment credit. Continue?",
                PrimaryButtonText = "Yes, enroll for free",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };

            await Task.Delay(150);

            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && currentUser is not null)
            {
                await App.Services.AmbassadorRepository!.DecrementRewardBalanceAsync(currentUser.Id);
                return true;
            }

            return false;
        };

        model.ShowSeatGuideAction = async () =>
        {
            closeDialogAction();
            await Task.Delay(150);
            int capacity = movieEvent.MaxCapacity > 0 ? movieEvent.MaxCapacity : 50;
            SeatGuideDialog seatDialog = new SeatGuideDialog(capacity) { XamlRoot = xamlRoot };
            await seatDialog.ShowAsync();
        };

        return model;
    }
}