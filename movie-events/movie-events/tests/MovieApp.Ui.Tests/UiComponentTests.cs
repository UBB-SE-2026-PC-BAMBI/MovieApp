// <copyright file="UiComponentTests.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MovieApp.Core.Models;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Converters;
using System;
using System.Globalization;
using Windows.UI;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class BoolToHighlightBrushConverterTests
{
    private readonly BoolToHighlightBrushConverter _converter = new();

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(
            () => _converter.ConvertBack(null!, typeof(bool), null!, string.Empty));
    }

}

public sealed class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_True_ReturnsVisible()
    {
        var result = _converter.Convert(true, typeof(Visibility), null!, string.Empty);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_False_ReturnsCollapsed()
    {
        var result = _converter.Convert(false, typeof(Visibility), null!, string.Empty);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NonBool_ReturnsCollapsed()
    {
        var result = _converter.Convert("notabool", typeof(Visibility), null!, string.Empty);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        var result = _converter.Convert(null!, typeof(Visibility), null!, string.Empty);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ConvertBack_Visible_ReturnsTrue()
    {
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null!, string.Empty);

        Assert.Equal(true, result);
    }

    [Fact]
    public void ConvertBack_Collapsed_ReturnsFalse()
    {
        var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, string.Empty);

        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBack_NonVisibility_ReturnsFalse()
    {
        var result = _converter.ConvertBack("not a visibility", typeof(bool), null!, string.Empty);

        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBack_Null_ReturnsFalse()
    {
        var result = _converter.ConvertBack(null!, typeof(bool), null!, string.Empty);

        Assert.Equal(false, result);
    }

    [Theory]
    [InlineData(true, Visibility.Visible)]
    [InlineData(false, Visibility.Collapsed)]
    public void Convert_BoolValues_RoundTrip(bool input, Visibility expected)
    {
        var forward = _converter.Convert(input, typeof(Visibility), null!, string.Empty);
        Assert.Equal(expected, forward);

        var back = _converter.ConvertBack(expected, typeof(bool), null!, string.Empty);
        Assert.Equal(input, back);
    }
}

public sealed class NegativeBooleanConverterExtendedTests
{
    private readonly NegativeBooleanConverter _converter = new();

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Convert_Bool_Inverts(bool input, bool expected)
    {
        var result = _converter.Convert(input, typeof(bool), null!, string.Empty);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBack_Bool_Inverts(bool input, bool expected)
    {
        var result = _converter.ConvertBack(input, typeof(bool), null!, string.Empty);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NonBool_PassesThrough()
    {
        var result = _converter.Convert(42, typeof(int), null!, string.Empty);
        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertBack_NonBool_PassesThrough()
    {
        var result = _converter.ConvertBack("hello", typeof(string), null!, string.Empty);
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Convert_Null_ReturnsNull()
    {
        var result = _converter.Convert(null!, typeof(object), null!, string.Empty);
        Assert.Null(result);
    }

    [Fact]
    public void ConvertBack_Null_ReturnsNull()
    {
        var result = _converter.ConvertBack(null!, typeof(object), null!, string.Empty);
        Assert.Null(result);
    }
}

public sealed class SeatGuideDialogStaticTests
{
    [Fact]
    public void GetVisibility_True_ReturnsVisible()
    {
        Assert.Equal(Visibility.Visible, SeatGuideDialog.GetVisibility(true));
    }

    [Fact]
    public void GetVisibility_False_ReturnsCollapsed()
    {
        Assert.Equal(Visibility.Collapsed, SeatGuideDialog.GetVisibility(false));
    }

    [Fact]
    public void GetInverseVisibility_True_ReturnsCollapsed()
    {
        Assert.Equal(Visibility.Collapsed, SeatGuideDialog.GetInverseVisibility(true));
    }

    [Fact]
    public void GetInverseVisibility_False_ReturnsVisible()
    {
        Assert.Equal(Visibility.Visible, SeatGuideDialog.GetInverseVisibility(false));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetVisibility_And_GetInverseVisibility_AreOpposites(bool value)
    {
        var normal = SeatGuideDialog.GetVisibility(value);
        var inverse = SeatGuideDialog.GetInverseVisibility(value);

        Assert.NotEqual(normal, inverse);
    }
}

public sealed class EventCardFormattingTests
{
    private static readonly CultureInfo UsEn = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    // ── Null-guard fallbacks ──────────────────────────────────────────

    [Fact]
    public void GetTitleText_Null_ReturnsFallback()
        => Assert.Equal("Untitled event", EventCard.GetTitleText(null));

    [Fact]
    public void GetDescriptionText_Null_ReturnsFallback()
        => Assert.Equal("A curated movie experience with limited seating.", EventCard.GetDescriptionText(null));

    [Fact]
    public void GetEventTypeText_Null_ReturnsFallback()
        => Assert.Equal("Special Event", EventCard.GetEventTypeText(null));

    [Fact]
    public void GetDateBadgeDay_Null_ReturnsDashes()
        => Assert.Equal("--", EventCard.GetDateBadgeDay(null, Invariant));

    [Fact]
    public void GetScheduleText_Null_ReturnsFallback()
        => Assert.Equal("Schedule to be announced", EventCard.GetScheduleText(null, Invariant));

    [Fact]
    public void GetLocationText_Null_ReturnsFallback()
        => Assert.Equal("Location to be announced", EventCard.GetLocationText(null));

    [Fact]
    public void GetPriceText_Null_ReturnsDash()
        => Assert.Equal("-", EventCard.GetPriceText(null, Invariant));

    [Fact]
    public void GetRatingText_Null_ReturnsDash()
        => Assert.Equal("-", EventCard.GetRatingText(null));

    [Fact]
    public void GetCapacityText_Null_ReturnsDash()
        => Assert.Equal("-", EventCard.GetCapacityText(null));

    [Fact]
    public void GetStatusText_Null_ReturnsPending()
        => Assert.Equal("Pending", EventCard.GetStatusText(null, DateTime.UnixEpoch));


    [Fact]
    public void GetTitleText_ValidEvent_ReturnsTitle()
        => Assert.Equal("Galaxy Night", EventCard.GetTitleText(MakeEvent(title: "Galaxy Night")));


    [Fact]
    public void GetDescriptionText_WhitespaceDescription_ReturnsFallback()
        => Assert.Equal(
            "A curated movie experience with limited seating.",
            EventCard.GetDescriptionText(MakeEvent(description: "   ")));

    [Fact]
    public void GetDescriptionText_ValidDescription_ReturnsDescription()
        => Assert.Equal("Exclusive night.", EventCard.GetDescriptionText(MakeEvent(description: "Exclusive night.")));


    [Fact]
    public void GetEventTypeText_Trimmed_ReturnsTrimmedValue()
        => Assert.Equal("Premiere", EventCard.GetEventTypeText(MakeEvent(eventType: "  Premiere  ")));

    [Fact]
    public void GetEventTypeText_EmptyString_ReturnsFallback()
        => Assert.Equal("Special Event", EventCard.GetEventTypeText(MakeEvent(eventType: "")));


    [Fact]
    public void GetDateBadgeDay_ValidDate_ReturnsTwoDigitDay()
        => Assert.Equal("07", EventCard.GetDateBadgeDay(MakeEvent(dt: new DateTime(2030, 3, 7)), Invariant));


    [Fact]
    public void GetScheduleText_ValidDate_ContainsDayAndTime()
    {
        var dt = new DateTime(2030, 5, 8, 19, 30, 0);
        var text = EventCard.GetScheduleText(MakeEvent(dt: dt), Invariant);
        Assert.Contains("May", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("8", text);
    }


    [Fact]
    public void GetLocationText_WhitespaceLocation_ReturnsFallback()
        => Assert.Equal("Location to be announced", EventCard.GetLocationText(MakeEvent(location: "  ")));

    [Fact]
    public void GetLocationText_ValidLocation_ReturnsLocation()
        => Assert.Equal("Grand Hall", EventCard.GetLocationText(MakeEvent(location: "Grand Hall")));


    [Fact]
    public void GetPriceText_ZeroPrice_ReturnsFree()
        => Assert.Equal("Free", EventCard.GetPriceText(MakeEvent(price: 0), UsEn));

    [Fact]
    public void GetPriceText_NegativePrice_ReturnsFree()
        => Assert.Equal("Free", EventCard.GetPriceText(MakeEvent(price: -5), UsEn));

    [Fact]
    public void GetPriceText_PositivePrice_ReturnsCurrencyString()
        => Assert.Equal("$25.00", EventCard.GetPriceText(MakeEvent(price: 25), UsEn));


    [Fact]
    public void GetDiscountedPriceText_NullEvent_ReturnsDash()
        => Assert.Equal("-", EventCard.GetDiscountedPriceText(null, UsEn, 10));

    [Fact]
    public void GetDiscountedPriceText_ZeroPrice_ReturnsFree()
        => Assert.Equal("Free", EventCard.GetDiscountedPriceText(MakeEvent(price: 0), UsEn, 50));

    [Fact]
    public void GetDiscountedPriceText_ZeroDiscount_ReturnsFullPrice()
        => Assert.Equal("$20.00", EventCard.GetDiscountedPriceText(MakeEvent(price: 20), UsEn, 0));

    [Fact]
    public void GetDiscountedPriceText_50PercentDiscount_ReturnsHalfPrice()
    {
        var text = EventCard.GetDiscountedPriceText(MakeEvent(price: 20), UsEn, 50);
        Assert.Contains("$10.00", text);
        Assert.Contains("-50%", text);
    }

    [Fact]
    public void GetDiscountedPriceText_NegativeDiscount_TreatsAsZero()
        => Assert.Equal("$20.00", EventCard.GetDiscountedPriceText(MakeEvent(price: 20), UsEn, -10));

    [Fact]
    public void GetDiscountedPriceText_FullDiscount_ReturnsZeroPrice()
    {
        var text = EventCard.GetDiscountedPriceText(MakeEvent(price: 100), UsEn, 100);
        Assert.Contains("$0.00", text);
        Assert.Contains("-100%", text);
    }


    [Fact]
    public void GetRatingText_ZeroRating_ReturnsNew()
        => Assert.Equal("New", EventCard.GetRatingText(MakeEvent(rating: 0)));

    [Fact]
    public void GetRatingText_NegativeRating_ReturnsNew()
        => Assert.Equal("New", EventCard.GetRatingText(MakeEvent(rating: -1)));

    [Fact]
    public void GetRatingText_ValidRating_ReturnsFormattedOutOfFive()
        => Assert.Equal("4.7/5", EventCard.GetRatingText(MakeEvent(rating: 4.7)));

    [Fact]
    public void GetRatingText_MaxRating_Returns5OutOf5()
        => Assert.Equal("5.0/5", EventCard.GetRatingText(MakeEvent(rating: 5.0)));


    [Fact]
    public void GetCapacityText_ValidValues_ReturnsFraction()
        => Assert.Equal("40/100", EventCard.GetCapacityText(MakeEvent(enrolled: 40, capacity: 100)));

    [Fact]
    public void GetCapacityText_FullHouse_ShowsMaxEquals()
        => Assert.Equal("100/100", EventCard.GetCapacityText(MakeEvent(enrolled: 100, capacity: 100)));


    [Fact]
    public void GetStatusText_EventEnded_ReturnsEnded()
    {
        var now = new DateTime(2030, 6, 1);
        var ev = MakeEvent(dt: now.AddHours(-1), enrolled: 10, capacity: 100);
        Assert.Equal("Ended", EventCard.GetStatusText(ev, now));
    }

    [Fact]
    public void GetStatusText_SoldOut_ReturnsSoldOut()
    {
        var now = new DateTime(2030, 6, 1);
        var ev = MakeEvent(dt: now.AddDays(1), enrolled: 100, capacity: 100);
        Assert.Equal("Sold out", EventCard.GetStatusText(ev, now));
    }

    [Fact]
    public void GetStatusText_OneSpotLeft_ReturnsOneSpotLeft()
    {
        var now = new DateTime(2030, 6, 1);
        var ev = MakeEvent(dt: now.AddDays(1), enrolled: 99, capacity: 100);
        Assert.Equal("1 spot left", EventCard.GetStatusText(ev, now));
    }

    [Fact]
    public void GetStatusText_ManySpots_ReturnsPluralSpots()
    {
        var now = new DateTime(2030, 6, 1);
        var ev = MakeEvent(dt: now.AddDays(1), enrolled: 50, capacity: 100);
        Assert.Equal("50 spots left", EventCard.GetStatusText(ev, now));
    }

    [Fact]
    public void GetStatusText_ExactlyNow_ReturnsEnded()
    {
        var now = new DateTime(2030, 6, 1, 12, 0, 0);
        var ev = MakeEvent(dt: now, enrolled: 10, capacity: 100);
        Assert.Equal("Ended", EventCard.GetStatusText(ev, now));
    }


    private static Event MakeEvent(
        string title = "Test Event",
        string description = "Description",
        string eventType = "Festival",
        string location = "Hall",
        DateTime? dt = null,
        decimal price = 20m,
        double rating = 4.5,
        int enrolled = 10,
        int capacity = 50)
    {
        return new Event
        {
            Id = 1,
            Title = title,
            Description = description,
            PosterUrl = string.Empty,
            EventDateTime = dt ?? DateTime.Now.AddDays(1),
            LocationReference = location,
            TicketPrice = price,
            HistoricalRating = rating,
            EventType = eventType,
            MaxCapacity = capacity,
            CurrentEnrollment = enrolled,
            CreatorUserId = 1,
        };
    }
}


public sealed class EventCardCultureTests
{
    [Fact]
    public void GetPriceText_DifferentCultures_ReturnsFormattedPrice()
    {
        var ev = new Event
        {
            Id = 1,
            Title = "T",
            PosterUrl = string.Empty,
            EventDateTime = DateTime.Now.AddDays(1),
            LocationReference = "L",
            TicketPrice = 99.99m,
            CreatorUserId = 1,
        };

        var usText = EventCard.GetPriceText(ev, CultureInfo.GetCultureInfo("en-US"));
        Assert.Contains("99.99", usText);
    }

    [Fact]
    public void GetDiscountedPriceText_WithDiscount_AppendsBadge()
    {
        var ev = new Event
        {
            Id = 1,
            Title = "T",
            PosterUrl = string.Empty,
            EventDateTime = DateTime.Now.AddDays(1),
            LocationReference = "L",
            TicketPrice = 100m,
            CreatorUserId = 1,
        };

        var text = EventCard.GetDiscountedPriceText(ev, CultureInfo.GetCultureInfo("en-US"), 25);
        Assert.Contains("(-25%)", text);
        Assert.Contains("$75.00", text);
    }
}


public sealed class EventSearchBoxLogicTests
{


    [Fact]
    public void SearchTextProperty_IsRegistered()
    {
        var prop = EventSearchBox.SearchTextProperty;
        Assert.NotNull(prop);
    }

    [Fact]
    public void PlaceholderTextProperty_IsRegistered()
    {
        var prop = EventSearchBox.PlaceholderTextProperty;
        Assert.NotNull(prop);
    }

    [Fact]
    public void SearchTextProperty_DefaultValue_IsEmptyString()
    {
        var meta = EventSearchBox.SearchTextProperty.GetMetadata(typeof(EventSearchBox));
        Assert.Equal(string.Empty, meta.DefaultValue);
    }

    [Fact]
    public void PlaceholderTextProperty_DefaultValue_IsSearchEvents()
    {
        var meta = EventSearchBox.PlaceholderTextProperty.GetMetadata(typeof(EventSearchBox));
        Assert.Equal("Search events", meta.DefaultValue);
    }
}

public sealed class EventSortSelectorLogicTests
{
    [Fact]
    public void SelectedSortOptionProperty_IsRegistered()
    {
        var prop = EventSortSelector.SelectedSortOptionProperty;
        Assert.NotNull(prop);
    }

    [Fact]
    public void PlaceholderTextProperty_IsRegistered()
    {
        var prop = EventSortSelector.PlaceholderTextProperty;
        Assert.NotNull(prop);
    }

    [Fact]
    public void PlaceholderTextProperty_DefaultValue_IsSortEvents()
    {
        var meta = EventSortSelector.PlaceholderTextProperty.GetMetadata(typeof(EventSortSelector));
        Assert.Equal("Sort events", meta.DefaultValue);
    }

    [Fact]
    public void SelectedSortOptionProperty_DefaultValue_IsDateAscending()
    {
        var meta = EventSortSelector.SelectedSortOptionProperty.GetMetadata(typeof(EventSortSelector));
        Assert.Equal(MovieApp.Core.EventLists.EventSortOption.DateAscending, meta.DefaultValue);
    }
}

public sealed class SectionHeaderLogicTests
{
    [Fact]
    public void TitleProperty_IsRegistered()
    {
        var prop = SectionHeader.TitleProperty;
        Assert.NotNull(prop);
    }

    [Fact]
    public void ActionTextProperty_IsRegistered()
    {
        var prop = SectionHeader.ActionTextProperty;
        Assert.NotNull(prop);
    }

    [Fact]
    public void TitleProperty_DefaultValue_IsEmpty()
    {
        var meta = SectionHeader.TitleProperty.GetMetadata(typeof(SectionHeader));
        Assert.Equal(string.Empty, meta.DefaultValue);
    }

    [Fact]
    public void ActionTextProperty_DefaultValue_IsSeeAll()
    {
        var meta = SectionHeader.ActionTextProperty.GetMetadata(typeof(SectionHeader));
        Assert.Equal("See all", meta.DefaultValue);
    }
}

