using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MovieApp.Core.Models;
using MovieApp.Ui.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MovieApp.Ui.Tests")]

namespace MovieApp.Ui.Controls;

public sealed partial class EventCard : UserControl
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model),
        typeof(object),
        typeof(EventCard),
        new PropertyMetadata(null, OnEventChanged));

    public static readonly DependencyProperty DiscountPercentageProperty = DependencyProperty.Register(
        nameof(DiscountPercentage),
        typeof(int),
        typeof(EventCard),
        new PropertyMetadata(0, OnComputedPropertyChanged));

    public static readonly DependencyProperty IsJoinedProperty = DependencyProperty.Register(
        nameof(IsJoined),
        typeof(bool),
        typeof(EventCard),
        new PropertyMetadata(false, OnComputedPropertyChanged));

    public static readonly DependencyProperty IsFavoritedProperty = DependencyProperty.Register(
        nameof(IsFavorited),
        typeof(bool),
        typeof(EventCard),
        new PropertyMetadata(false, OnComputedPropertyChanged));

    public EventCard()
    {
        InitializeComponent();
    }

    public object? Model
    {
        get => GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public int DiscountPercentage
    {
        get => (int)GetValue(DiscountPercentageProperty);
        set => SetValue(DiscountPercentageProperty, value);
    }

    public bool IsJoined
    {
        get => (bool)GetValue(IsJoinedProperty);
        set => SetValue(IsJoinedProperty, value);
    }

    public bool IsFavorited
    {
        get => (bool)GetValue(IsFavoritedProperty);
        set => SetValue(IsFavoritedProperty, value);
    }

    private Event? EventModel => Model as Event;

    public string TitleText => GetTitleText(EventModel);
    public string DescriptionText => GetDescriptionText(EventModel);
    public string EventTypeText => GetEventTypeText(EventModel);
    public string DateBadgeDay => GetDateBadgeDay(EventModel, CultureInfo.CurrentCulture);
    public string ScheduleText => GetScheduleText(EventModel, CultureInfo.CurrentCulture);
    public string LocationText => GetLocationText(EventModel);
    public string PriceText => GetDiscountedPriceText(EventModel, CultureInfo.CurrentCulture, DiscountPercentage);
    public string RatingText => GetRatingText(EventModel);
    public string CapacityText => GetCapacityText(EventModel);
    public string StatusText => GetStatusText(EventModel, DateTime.Now);

    public string FavoriteIconGlyph => IsFavorited ? "\uEB52" : "\uEB51";

    public Brush FavoriteIconForeground => IsFavorited
        ? new SolidColorBrush(Microsoft.UI.Colors.Red)
        : (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];

    public Brush StatusBackgroundBrush
    {
        get
        {
            if (EventModel is null)
            {
                return (Brush)Resources["StatusPendingBrush"];
            }

            if (EventModel.EventDateTime <= DateTime.Now)
            {
                return (Brush)Resources["StatusEndedBrush"];
            }

            if (EventModel.AvailableSpots <= 0)
            {
                return (Brush)Resources["StatusSoldOutBrush"];
            }

            return (Brush)Resources["StatusAvailableBrush"];
        }
    }

    public bool HasDiscount => DiscountPercentage > 0;
    public Visibility DiscountBadgeVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;
    public Visibility JoinButtonVisibility => IsJoined ? Visibility.Collapsed : Visibility.Visible;
    public Visibility JoinedStatusVisibility => IsJoined ? Visibility.Visible : Visibility.Collapsed;
    public string DiscountBadgeText => $"-{DiscountPercentage}%";

    internal static string GetTitleText(Event? movieEvent) => movieEvent?.Title ?? "Untitled event";

    internal static string GetDescriptionText(Event? movieEvent) => string.IsNullOrWhiteSpace(movieEvent?.Description)
        ? "A curated movie experience with limited seating."
        : movieEvent.Description!;

    internal static string GetEventTypeText(Event? movieEvent) => string.IsNullOrWhiteSpace(movieEvent?.EventType)
        ? "Special Event"
        : movieEvent.EventType.Trim();

    internal static string GetDateBadgeDay(Event? movieEvent, CultureInfo culture) => movieEvent is null
        ? "--"
        : movieEvent.EventDateTime.ToString("dd", culture);

    internal static string GetScheduleText(Event? movieEvent, CultureInfo culture) => movieEvent is null
        ? "Schedule to be announced"
        : movieEvent.EventDateTime.ToString("ddd, MMM d • h:mm tt", culture);

    internal static string GetLocationText(Event? movieEvent) => string.IsNullOrWhiteSpace(movieEvent?.LocationReference)
        ? "Location to be announced"
        : movieEvent.LocationReference;

    internal static string GetPriceText(Event? movieEvent, CultureInfo culture) => movieEvent is null
        ? "-"
        : movieEvent.TicketPrice <= 0
            ? "Free"
            : movieEvent.TicketPrice.ToString("C", culture);

    internal static string GetDiscountedPriceText(Event? movieEvent, CultureInfo culture, int discountPercent)
    {
        if (movieEvent is null) return "-";
        if (movieEvent.TicketPrice <= 0) return "Free";
        if (discountPercent <= 0) return movieEvent.TicketPrice.ToString("C", culture);
        decimal discounted = movieEvent.TicketPrice * (1 - discountPercent / 100m);
        return $"{discounted.ToString("C", culture)} (-{discountPercent}%)";
    }

    internal static string GetRatingText(Event? movieEvent) => movieEvent is null
        ? "-"
        : movieEvent.HistoricalRating <= 0
            ? "New"
            : $"{movieEvent.HistoricalRating:0.0}/5";

    internal static string GetCapacityText(Event? movieEvent) => movieEvent is null
        ? "-"
        : $"{movieEvent.CurrentEnrollment}/{movieEvent.MaxCapacity}";

    internal static string GetStatusText(Event? movieEvent, DateTime now)
    {
        if (movieEvent is null) return "Pending";
        if (movieEvent.EventDateTime <= now) return "Ended";
        if (movieEvent.AvailableSpots <= 0) return "Sold out";
        return movieEvent.AvailableSpots == 1 ? "1 spot left" : $"{movieEvent.AvailableSpots} spots left";
    }

    private static void OnEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is EventCard card)
        {
            card.IsJoined = false;
            card.IsFavorited = false;
            card.RefreshComputedProperties();
        }
    }

    private static void OnComputedPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is EventCard card)
        {
            card.Bindings.Update();
        }
    }

    private async void WatcherButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (EventModel == null || App.Services.WatchlistPathProvider == null) return;

            ToggleButton button = (ToggleButton)sender;
            bool isWatching = button.IsChecked ?? false;
            string folderPath = App.Services.WatchlistPathProvider.GetWatchlistFolderPath();
            MovieApp.Infrastructure.LocalPriceWatcherRepository repo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);

            if (isWatching)
            {
                TextBox inputTextBox = new TextBox { PlaceholderText = "Ex: 50.00", Width = 200 };
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Set Target Price",
                    Content = new StackPanel
                    {
                        Spacing = 10,
                        Children = { new TextBlock { Text = "Enter desired target price:" }, inputTextBox }
                    },
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string cleanInput = inputTextBox.Text.Replace(",", ".");
                    if (decimal.TryParse(cleanInput, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal targetPrice))
                    {
                        bool success = await repo.AddWatchAsync(new WatchedEvent { EventId = EventModel.Id, EventTitle = EventModel.Title, TargetPrice = targetPrice });
                        if (!success)
                        {
                            button.IsChecked = false;
                            ContentDialog errorDialog = new ContentDialog
                            {
                                Title = "Limit Reached",
                                Content = "You can only watch up to 10 events, or you already watch this one.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                        }
                    }
                    else
                    {
                        button.IsChecked = false;
                    }
                }
                else
                {
                    button.IsChecked = false;
                }
            }
            else
            {
                await repo.RemoveWatchAsync(EventModel.Id);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (EventModel == null || App.Services.FavoriteEventService == null) return;

        if (IsFavorited)
        {
            await App.Services.FavoriteEventService.RemoveFavoriteAsync(App.CurrentUserId, EventModel.Id);
        }
        else
        {
            await App.Services.FavoriteEventService.AddFavoriteAsync(App.CurrentUserId, EventModel.Id);
        }

        IsFavorited = !IsFavorited;
        Bindings.Update();
    }

    private async Task SyncWatcherStateAsync()
    {
        if (EventModel == null || WatcherButton == null || App.Services.WatchlistPathProvider == null) return;
        string folderPath = App.Services.WatchlistPathProvider.GetWatchlistFolderPath();
        MovieApp.Infrastructure.LocalPriceWatcherRepository repo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);
        WatcherButton.IsChecked = await repo.IsWatchingAsync(EventModel.Id);
    }

    private async Task SyncJoinedStateAsync()
    {
        if (EventModel == null || App.Services.EventUserStateService == null) return;
        bool joined = await App.Services.EventUserStateService.IsEventJoinedByUserAsync(EventModel.Id);
        IsJoined = joined;
        Bindings.Update();
    }

    private async Task SyncFavoriteStateAsync()
    {
        if (EventModel == null || App.Services.FavoriteEventService == null) return;
        IsFavorited = await App.Services.FavoriteEventService.ExistsFavoriteAsync(App.CurrentUserId, EventModel.Id);
        Bindings.Update();
    }

    private void RefreshComputedProperties()
    {
        Bindings.Update();
        _ = SyncWatcherStateAsync();
        _ = SyncJoinedStateAsync();
        _ = SyncFavoriteStateAsync();
    }

    private async void JoinEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (EventModel is null || App.Services.EventJoinService is null) return;
        Button button = (Button)sender;
        button.IsEnabled = false;
        string tag = button.Tag?.ToString() ?? string.Empty;
        JoinEventResult result = await App.Services.EventJoinService.JoinEventAsync(EventModel.Id, tag);

        if (result.Success)
        {
            IsJoined = true;
            Bindings.Update();
        }
        else
        {
            button.IsEnabled = true;
        }
    }
}