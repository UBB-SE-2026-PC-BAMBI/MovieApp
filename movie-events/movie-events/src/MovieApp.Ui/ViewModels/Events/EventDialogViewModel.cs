using MovieApp.Core.Models;

namespace MovieApp.Ui.Services;

public class EventDialogViewModel
{
    public Event Event { get; }
    public bool IsJackpotEvent { get; }
    public int? DiscountPercent { get; }

    public string Description { get; set; } = string.Empty;
    public string FormattedDate { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PriceText { get; set; } = string.Empty;
    public string RatingText { get; set; } = string.Empty;
    public string CapacityText { get; set; } = string.Empty;

    public bool ShowJackpotBanner { get; set; }
    public string JackpotBannerText { get; set; } = string.Empty;
    public string JackpotDiscountedPriceText { get; set; } = string.Empty;

    public bool ShowRegularDiscountBanner { get; set; }
    public string RegularDiscountedPriceText { get; set; } = string.Empty;

    public bool HasFreePass { get; set; }
    public int FreePassBalance { get; set; }

    public Func<string, Task<bool>>? ValidateReferralAction { get; set; }
    public Func<Task<bool>>? UseFreePassAction { get; set; }
    public Action? ShowSeatGuideAction { get; set; }

    public EventDialogViewModel(Event @event, bool isJackpotEvent, int? discountPercent)
    {
        Event = @event;
        IsJackpotEvent = isJackpotEvent;
        DiscountPercent = discountPercent;
    }
}