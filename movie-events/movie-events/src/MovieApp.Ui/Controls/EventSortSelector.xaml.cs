// <copyright file="EventSortSelector.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Controls;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.EventLists;

/// <summary>
/// Reusable single-select sort control for event-list screens.
/// </summary>
/// <remarks>
/// This control exposes the shared event sort modes used by all list screens and
/// ensures only one sort option is selected at a time through its combo-box UI.
/// </remarks>
public sealed partial class EventSortSelector : UserControl
{
    /// <summary>
    /// Identifies the <see cref="SelectedSortOption"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedSortOptionProperty =
        DependencyProperty.Register(
            nameof(SelectedSortOption),
            typeof(EventSortOption),
            typeof(EventSortSelector),
            new PropertyMetadata(EventSortOption.DateAscending, OnSelectedSortOptionChanged));

    /// <summary>
    /// Identifies the <see cref="PlaceholderText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(EventSortSelector),
            new PropertyMetadata("Sort events"));

    private static readonly IReadOnlyList<EventSortOptionItem> DefaultSortOptions =
    [
        new EventSortOptionItem(EventSortOption.DateAscending, "Date: soonest first"),
        new EventSortOptionItem(EventSortOption.DateDescending, "Date: latest first"),
        new EventSortOptionItem(EventSortOption.PriceAscending, "Price: low to high"),
        new EventSortOptionItem(EventSortOption.PriceDescending, "Price: high to low"),
        new EventSortOptionItem(EventSortOption.HistoricalRatingDescending, "Historical rating")
    ];

    private bool isLoaded;
    private EventSortOption pendingSortOption = EventSortOption.DateAscending;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSortSelector"/> class.
    /// </summary>
    public EventSortSelector()
    {
        this.InitializeComponent();
        this.Loaded += this.OnLoaded;
    }

    /// <summary>
    /// Raised when the user selects a different sort mode.
    /// </summary>
    public event EventHandler<EventSortOption>? SortOptionChanged;

    /// <summary>
    /// Gets or sets the currently selected event sort option.
    /// </summary>
    public EventSortOption SelectedSortOption
    {
        get => (EventSortOption)this.GetValue(SelectedSortOptionProperty);
        set => this.SetValue(SelectedSortOptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text shown by the selector before a value is chosen.
    /// </summary>
    public string PlaceholderText
    {
        get => (string)this.GetValue(PlaceholderTextProperty);
        set => this.SetValue(PlaceholderTextProperty, value);
    }

    private static void OnSelectedSortOptionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not EventSortSelector selector || args.NewValue is not EventSortOption sortOption)
        {
            return;
        }

        selector.pendingSortOption = sortOption;
        selector.UpdateSelectedItem(sortOption);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.isLoaded = true;
        this.SortComboBox.ItemsSource = DefaultSortOptions;
        this.UpdateSelectedItem(this.pendingSortOption);
    }

    /// <summary>
    /// Synchronizes the selected combo-box item with the dependency property value.
    /// </summary>
    private void UpdateSelectedItem(EventSortOption sortOption)
    {
        if (!this.isLoaded || this.SortComboBox is null)
        {
            return;
        }

        EventSortOptionItem? selectedItem =
            DefaultSortOptions.FirstOrDefault(item => item.Value == sortOption);
        if (!ReferenceEquals(this.SortComboBox.SelectedItem, selectedItem))
        {
            this.SortComboBox.SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// Propagates the chosen sort mode back to the host page.
    /// </summary>
    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.SortComboBox.SelectedItem is not EventSortOptionItem selectedItem)
        {
            return;
        }

        if (this.SelectedSortOption != selectedItem.Value)
        {
            this.SelectedSortOption = selectedItem.Value;
        }

        this.SortOptionChanged?.Invoke(this, selectedItem.Value);
    }

    /// <summary>
    /// Represents a single selectable sort option item.
    /// </summary>
    private sealed class EventSortOptionItem
    {
        public EventSortOptionItem(EventSortOption value, string label)
        {
            this.Value = value;
            this.Label = label;
        }

        public EventSortOption Value { get; }

        public string Label { get; }
    }
}
