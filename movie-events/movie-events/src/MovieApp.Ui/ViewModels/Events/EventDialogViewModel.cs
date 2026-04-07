// <copyright file="EventDialogViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Holds the display data and interaction callbacks for the event detail dialog.
/// </summary>
public class EventDialogViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventDialogViewModel"/> class.
    /// </summary>
    /// <param name="event">The event being displayed.</param>
    /// <param name="isJackpotEvent">Whether this is a jackpot-linked event.</param>
    /// <param name="discountPercent">The optional discount percentage to display.</param>
    public EventDialogViewModel(Event @event, bool isJackpotEvent, int? discountPercent)
    {
        this.Event = @event;
        this.IsJackpotEvent = isJackpotEvent;
        this.DiscountPercent = discountPercent;
    }

    /// <summary>Gets the event being displayed.</summary>
    public Event Event { get; }

    /// <summary>Gets a value indicating whether this is a jackpot event.</summary>
    public bool IsJackpotEvent { get; }

    /// <summary>Gets the optional discount percentage.</summary>
    public int? DiscountPercent { get; }

    /// <summary>Gets or sets the event description text.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the formatted event date string.</summary>
    public string FormattedDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the event location string.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets the ticket price text.</summary>
    public string PriceText { get; set; } = string.Empty;

    /// <summary>Gets or sets the event rating text.</summary>
    public string RatingText { get; set; } = string.Empty;

    /// <summary>Gets or sets the event capacity text.</summary>
    public string CapacityText { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the jackpot banner should be shown.</summary>
    public bool ShowJackpotBanner { get; set; }

    /// <summary>Gets or sets the jackpot banner text.</summary>
    public string JackpotBannerText { get; set; } = string.Empty;

    /// <summary>Gets or sets the jackpot discounted price text.</summary>
    public string JackpotDiscountedPriceText { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the regular discount banner should be shown.</summary>
    public bool ShowRegularDiscountBanner { get; set; }

    /// <summary>Gets or sets the regular discounted price text.</summary>
    public string RegularDiscountedPriceText { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the user has a free pass available.</summary>
    public bool HasFreePass { get; set; }

    /// <summary>Gets or sets the number of free passes remaining.</summary>
    public int FreePassBalance { get; set; }

    /// <summary>Gets or sets the referral code validation callback.</summary>
    public Func<string, Task<bool>>? ValidateReferralAction { get; set; }

    /// <summary>Gets or sets the free pass redemption callback.</summary>
    public Func<Task<bool>>? UseFreePassAction { get; set; }

    /// <summary>Gets or sets the seat guide display callback.</summary>
    public Action? ShowSeatGuideAction { get; set; }
}
