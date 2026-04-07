// <copyright file="EventCard.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MovieApp.Ui.Tests")]

namespace MovieApp.Ui.Controls;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using MovieApp.Core.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;

/// <summary>
/// Represents a UI card component that displays information about an event,
/// including pricing, availability, and user interaction states such as joined or favorited.
/// </summary>
public sealed partial class EventCard : UserControl
{
    /// <summary>
    /// Identifies the <see cref="Model"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model),
        typeof(object),
        typeof(EventCard),
        new PropertyMetadata(null, OnEventChanged));

    /// <summary>
    /// Identifies the <see cref="DiscountPercentage"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DiscountPercentageProperty = DependencyProperty.Register(
        nameof(DiscountPercentage),
        typeof(int),
        typeof(EventCard),
        new PropertyMetadata(0, OnComputedPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="IsJoined"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsJoinedProperty = DependencyProperty.Register(
        nameof(IsJoined),
        typeof(bool),
        typeof(EventCard),
        new PropertyMetadata(false, OnComputedPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="IsFavorited"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsFavoritedProperty = DependencyProperty.Register(
        nameof(IsFavorited),
        typeof(bool),
        typeof(EventCard),
        new PropertyMetadata(false, OnComputedPropertyChanged));

    /// <summary>
    /// Initializes a new instance of the <see cref="EventCard"/> class.
    /// </summary>
    public EventCard()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the underlying model object bound to the card.
    /// </summary>
    /// <returns>The bound model instance.</returns>
    public object? Model
    {
        get => this.GetValue(ModelProperty);
        set => this.SetValue(ModelProperty, value);
    }

    /// <summary>
    /// Gets or sets the discount percentage applied to the event price.
    /// </summary>
    /// <returns>An integer representing the discount percentage.</returns>
    public int DiscountPercentage
    {
        get => (int)this.GetValue(DiscountPercentageProperty);
        set => this.SetValue(DiscountPercentageProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user has joined the event.
    /// </summary>
    /// <returns><c>true</c> if joined; otherwise, <c>false</c>.</returns>
    public bool IsJoined
    {
        get => (bool)this.GetValue(IsJoinedProperty);
        set => this.SetValue(IsJoinedProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the event is marked as favorite.
    /// </summary>
    /// <returns><c>true</c> if favorited; otherwise, <c>false</c>.</returns>
    public bool IsFavorited
    {
        get => (bool)this.GetValue(IsFavoritedProperty);
        set => this.SetValue(IsFavoritedProperty, value);
    }

    /// <summary>Gets the formatted title text.</summary>
    public string TitleText => GetTitleText(this.EventModel);

    /// <summary>Gets the formatted description text.</summary>
    public string DescriptionText => GetDescriptionText(this.EventModel);

    /// <summary>Gets the formatted event type text.</summary>
    public string EventTypeText => GetEventTypeText(this.EventModel);

    /// <summary>Gets the formatted day badge text.</summary>
    public string DateBadgeDay => GetDateBadgeDay(this.EventModel, CultureInfo.CurrentCulture);

    /// <summary>Gets the formatted schedule text.</summary>
    public string ScheduleText => GetScheduleText(this.EventModel, CultureInfo.CurrentCulture);

    /// <summary>Gets the formatted location text.</summary>
    public string LocationText => GetLocationText(this.EventModel);

    /// <summary>Gets the formatted price text including discounts.</summary>
    public string PriceText =>
        GetDiscountedPriceText(this.EventModel, CultureInfo.CurrentCulture, this.DiscountPercentage);

    /// <summary>Gets the formatted rating text.</summary>
    public string RatingText => GetRatingText(this.EventModel);

    /// <summary>Gets the formatted capacity text.</summary>
    public string CapacityText => GetCapacityText(this.EventModel);

    /// <summary>Gets the current event status text.</summary>
    public string StatusText => GetStatusText(this.EventModel, DateTime.Now);

    /// <summary>
    /// Gets the glyph used for the favorite icon.
    /// </summary>
    public string FavoriteIconGlyph => this.IsFavorited ? "\uEB52" : "\uEB51";

    /// <summary>
    /// Gets the brush used for the favorite icon foreground.
    /// </summary>
    public Brush FavoriteIconForeground => this.IsFavorited
        ? new SolidColorBrush(Microsoft.UI.Colors.Red)
        : (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];

    /// <summary>
    /// Gets the background brush representing the event status.
    /// </summary>
    public Brush StatusBackgroundBrush
    {
        get
        {
            if (this.EventModel is null)
            {
                return (Brush)this.Resources["StatusPendingBrush"];
            }

            if (this.EventModel.EventDateTime <= DateTime.Now)
            {
                return (Brush)this.Resources["StatusEndedBrush"];
            }

            if (this.EventModel.AvailableSpots <= 0)
            {
                return (Brush)this.Resources["StatusSoldOutBrush"];
            }

            return (Brush)this.Resources["StatusAvailableBrush"];
        }
    }

    /// <summary>Gets a value indicating whether a discount is applied.</summary>
    public bool HasDiscount => this.DiscountPercentage > 0;

    /// <summary>Gets the visibility of the discount badge.</summary>
    public Visibility DiscountBadgeVisibility => this.HasDiscount
        ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Gets the visibility of the join button.</summary>
    public Visibility JoinButtonVisibility => this.IsJoined
        ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Gets the visibility of the joined status indicator.</summary>
    public Visibility JoinedStatusVisibility => this.IsJoined
        ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Gets the discount badge text.</summary>
    public string DiscountBadgeText => $"-{this.DiscountPercentage}%";

    /// <summary>
    /// Gets the event model cast from <see cref="Model"/>.
    /// </summary>
    private Event? EventModel => this.Model as Event;

    /// <summary>
    /// Returns the formatted title text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <returns>The title text.</returns>
    internal static string GetTitleText(Event? movieEvent) => movieEvent?.Title ?? "Untitled event";

    /// <summary>
    /// Returns the formatted description text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <returns>The description text.</returns>
    internal static string GetDescriptionText(Event? movieEvent) =>
        string.IsNullOrWhiteSpace(movieEvent?.Description)
            ? "A curated movie experience with limited seating."
            : movieEvent.Description!;

    /// <summary>
    /// Returns the formatted event type text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <returns>The event type text.</returns>
    internal static string GetEventTypeText(Event? movieEvent) =>
        string.IsNullOrWhiteSpace(movieEvent?.EventType)
            ? "Special Event"
            : movieEvent.EventType.Trim();

    /// <summary>
    /// Returns the formatted date badge day.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <param name="culture">The culture used for formatting.</param>
    /// <returns>The day string.</returns>
    internal static string GetDateBadgeDay(Event? movieEvent, CultureInfo culture) =>
        movieEvent is null ? "--" : movieEvent.EventDateTime.ToString("dd", culture);

    /// <summary>
    /// Returns the formatted schedule text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <param name="culture">The culture used for formatting.</param>
    /// <returns>The schedule string.</returns>
    internal static string GetScheduleText(Event? movieEvent, CultureInfo culture) =>
        movieEvent is null
            ? "Schedule to be announced"
            : movieEvent.EventDateTime.ToString("ddd, MMM d • h:mm tt", culture);

    /// <summary>
    /// Returns the formatted location text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <returns>The location string.</returns>
    internal static string GetLocationText(Event? movieEvent) =>
        string.IsNullOrWhiteSpace(movieEvent?.LocationReference)
            ? "Location to be announced"
            : movieEvent.LocationReference;

    /// <summary>
    /// Returns the formatted price text for the specified event.
    /// </summary>
    /// <param name="movieEvent">The event instance containing pricing information.</param>
    /// <param name="culture">The culture used for currency formatting.</param>
    /// <returns>
    /// A formatted price string:
    /// <list type="bullet">
    /// <item><description>"-" if the event is <c>null</c>.</description></item>
    /// <item><description>"Free" if the ticket price is less than or equal to zero.</description></item>
    /// <item><description>The formatted currency value otherwise.</description></item>
    /// </list>
    /// </returns>
    internal static string GetPriceText(Event? movieEvent, CultureInfo culture) =>
        movieEvent is null
            ? "-"
            : movieEvent.TicketPrice <= 0
                ? "Free"
                : movieEvent.TicketPrice.ToString("C", culture);

    /// <summary>
    /// Returns the formatted discounted price text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <param name="culture">The culture used for formatting.</param>
    /// <param name="discountPercent">The discount percentage.</param>
    /// <returns>The price string.</returns>
    internal static string GetDiscountedPriceText(Event? movieEvent, CultureInfo culture, int discountPercent)
    {
        if (movieEvent is null)
        {
            return "-";
        }

        if (movieEvent.TicketPrice <= 0)
        {
            return "Free";
        }

        if (discountPercent <= 0)
        {
            return movieEvent.TicketPrice.ToString("C", culture);
        }

        decimal discounted = movieEvent.TicketPrice * (1 - (discountPercent / 100m));
        return $"{discounted.ToString("C", culture)} (-{discountPercent}%)";
    }

    /// <summary>
    /// Returns the formatted rating text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <returns>The rating string.</returns>
    internal static string GetRatingText(Event? movieEvent) =>
        movieEvent is null
            ? "-"
            : movieEvent.HistoricalRating <= 0
                ? "New"
                : $"{movieEvent.HistoricalRating:0.0}/5";

    /// <summary>
    /// Returns the formatted capacity text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <returns>The capacity string.</returns>
    internal static string GetCapacityText(Event? movieEvent) =>
        movieEvent is null ? "-" : $"{movieEvent.CurrentEnrollment}/{movieEvent.MaxCapacity}";

    /// <summary>
    /// Returns the formatted status text.
    /// </summary>
    /// <param name="movieEvent">The event instance.</param>
    /// <param name="now">The current time reference.</param>
    /// <returns>The status string.</returns>
    internal static string GetStatusText(Event? movieEvent, DateTime now)
    {
        if (movieEvent is null)
        {
            return "Pending";
        }

        if (movieEvent.EventDateTime <= now)
        {
            return "Ended";
        }

        if (movieEvent.AvailableSpots <= 0)
        {
            return "Sold out";
        }

        return movieEvent.AvailableSpots == 1 ?
            "1 spot left" : $"{movieEvent.AvailableSpots} spots left";
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
            if (this.EventModel == null || App.Services.WatchlistPathProvider == null)
            {
                return;
            }

            ToggleButton button = (ToggleButton)sender;
            bool isWatching = button.IsChecked ?? false;

            string folderPath = App.Services.WatchlistPathProvider.GetWatchlistFolderPath();
            MovieApp.Infrastructure.LocalPriceWatcherRepository repo =
                new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);

            if (isWatching)
            {
                TextBox inputTextBox = new TextBox { PlaceholderText = "Ex: 50.00", Width = 200 };

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Set Target Price",
                    Content = new StackPanel
                    {
                        Spacing = 10,
                        Children =
                        {
                            new TextBlock { Text = "Enter desired target price:" },
                            inputTextBox,
                        },
                    },
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot,
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string cleanInput = inputTextBox.Text.Replace(",", ".");

                    if (decimal.TryParse(cleanInput, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal targetPrice))
                    {
                        bool success = await repo.AddWatchAsync(new WatchedEvent
                        {
                            EventId = this.EventModel.Id,
                            EventTitle = this.EventModel.Title,
                            TargetPrice = targetPrice,
                        });

                        if (!success)
                        {
                            button.IsChecked = false;

                            ContentDialog errorDialog = new ContentDialog
                            {
                                Title = "Limit Reached",
                                Content = "You can only watch up to 10 events, or you already watch this one.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot,
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
                await repo.RemoveWatchAsync(this.EventModel.Id);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.EventModel == null || App.Services.FavoriteEventService == null)
        {
            return;
        }

        if (this.IsFavorited)
        {
            await App.Services.FavoriteEventService
                .RemoveFavoriteAsync(App.CurrentUserId, this.EventModel.Id);
        }
        else
        {
            await App.Services.FavoriteEventService
                .AddFavoriteAsync(App.CurrentUserId, this.EventModel.Id);
        }

        this.IsFavorited = !this.IsFavorited;
        this.Bindings.Update();
    }

    private void RefreshComputedProperties()
    {
        this.Bindings.Update();
        _ = this.SyncWatcherStateAsync();
        _ = this.SyncJoinedStateAsync();
        _ = this.SyncFavoriteStateAsync();
    }

    private async Task SyncJoinedStateAsync()
    {
        if (this.EventModel == null || App.Services.EventUserStateService == null)
        {
            return;
        }

        bool joined =
            await App.Services.EventUserStateService.IsEventJoinedByUserAsync(this.EventModel.Id);
        this.IsJoined = joined;
        this.Bindings.Update();
    }

    private async Task SyncFavoriteStateAsync()
    {
        if (this.EventModel == null || App.Services.FavoriteEventService == null)
        {
            return;
        }

        this.IsFavorited = await App.Services.FavoriteEventService
            .ExistsFavoriteAsync(App.CurrentUserId, this.EventModel.Id);
        this.Bindings.Update();
    }

    private async Task SyncWatcherStateAsync()
    {
        if (this.EventModel == null || this.WatcherButton == null
            || App.Services.WatchlistPathProvider == null)
        {
            return;
        }

        string folderPath = App.Services.WatchlistPathProvider.GetWatchlistFolderPath();
        MovieApp.Infrastructure.LocalPriceWatcherRepository repo =
            new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);

        this.WatcherButton.IsChecked = await repo.IsWatchingAsync(this.EventModel.Id);
    }

    private async void JoinEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.EventModel is null || App.Services.EventJoinService is null)
        {
            return;
        }

        Button button = (Button)sender;
        button.IsEnabled = false;

        string tag = button.Tag?.ToString() ?? string.Empty;
        JoinEventResult result =
            await App.Services.EventJoinService.JoinEventAsync(this.EventModel.Id, tag);

        if (result.Success)
        {
            this.IsJoined = true;
            this.Bindings.Update();
        }
        else
        {
            button.IsEnabled = true;
        }
    }
}