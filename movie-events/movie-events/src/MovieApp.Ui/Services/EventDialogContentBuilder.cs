using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Ui.Controls;

namespace MovieApp.Ui.Services;

public sealed class EventDialogContentBuilder
{
    private readonly IReferralValidator? _referralValidator;
    private readonly ICurrentUserService? _currentUserService;

    public EventDialogContentBuilder(IReferralValidator? referralValidator, ICurrentUserService? currentUserService)
    {
        _referralValidator = referralValidator;
        _currentUserService = currentUserService;
    }

    public async Task<EventDialogViewModel> BuildAsync(
        Event @event,
        bool isJackpotEvent,
        int? discountPercent,
        Action closeDialogAction,
        XamlRoot xamlRoot)
    {
        EventDialogViewModel model = new EventDialogViewModel(@event, isJackpotEvent, discountPercent)
        {
            Description = @event.Description ?? "A curated movie experience with limited seating.",
            FormattedDate = $"When: {@event.EventDateTime:g}",
            Location = $"Where: {@event.LocationReference}",
            PriceText = $"Price: {EventCard.GetPriceText(@event, CultureInfo.CurrentCulture)}",
            RatingText = $"Rating: {EventCard.GetRatingText(@event)}",
            CapacityText = $"Seats: {EventCard.GetCapacityText(@event)}"
        };

        if (isJackpotEvent && discountPercent.HasValue)
        {
            model.ShowJackpotBanner = true;
            model.JackpotBannerText = $"{discountPercent.Value}% Jackpot Discount applies to this event!";
            model.JackpotDiscountedPriceText = $"Discounted price: {EventCard.GetDiscountedPriceText(@event, CultureInfo.CurrentCulture, discountPercent.Value)}";
        }
        else if (!isJackpotEvent && discountPercent > 0 && @event.TicketPrice > 0)
        {
            model.ShowRegularDiscountBanner = true;
            model.RegularDiscountedPriceText = $"Your price: {EventCard.GetDiscountedPriceText(@event, CultureInfo.CurrentCulture, discountPercent.Value)}";
        }

        User? currentUser = _currentUserService?.CurrentUser;

        if (App.AmbassadorRepository is not null && currentUser is not null)
        {
            int balance = await App.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
            if (balance > 0)
            {
                model.HasFreePass = true;
                model.FreePassBalance = balance;
            }
        }

        model.ValidateReferralAction = async (string code) =>
        {
            if (string.IsNullOrWhiteSpace(code) || _referralValidator is null || currentUser is null)
                return false;

            return await _referralValidator.IsValidReferralAsync(code, currentUser.Id);
        };

        model.UseFreePassAction = async () =>
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                XamlRoot = xamlRoot,
                Title = "Use Free Pass?",
                Content = "This will use 1 free enrollment credit. Continue?",
                PrimaryButtonText = "Yes, enroll for free",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary && currentUser is not null)
            {
                await App.AmbassadorRepository!.DecrementRewardBalanceAsync(currentUser.Id);
                return true;
            }
            return false;
        };

        model.ShowSeatGuideAction = async () =>
        {
            closeDialogAction();
            int capacity = @event.MaxCapacity > 0 ? @event.MaxCapacity : 50;
            SeatGuideDialog seatDialog = new SeatGuideDialog(capacity) { XamlRoot = xamlRoot };
            await seatDialog.ShowAsync();
        };

        return model;
    }
}