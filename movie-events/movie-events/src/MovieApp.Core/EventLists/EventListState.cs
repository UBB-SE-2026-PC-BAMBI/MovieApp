// <copyright file="EventListState.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.EventLists;

/// <summary>
/// Current search, sort, and filter state for an event list screen.
/// </summary>
public sealed class EventListState
{
    /// <summary>
    /// Gets or sets the free-text query applied to the current event list.
    /// </summary>
    /// <remarks>
    /// This value is owned by a single screen instance and is intended to filter only
    /// that screen's loaded <c>AllEvents</c> collection.
    /// </remarks>
    public string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currently selected sort option.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="EventSortOption.DateAscending"/> to ensure
    /// that users see the most immediate upcoming events first by default.
    /// </remarks>
    public EventSortOption SelectedSortOption { get; set; } = EventSortOption.DateAscending;

    /// <summary>
    /// Gets or sets the active filter criteria.
    /// </summary>
    public EventFilterState ActiveFilters { get; set; } = new EventFilterState();

    /// <summary>
    /// Gets the list of available sort options for the UI.
    /// </summary>
    public IReadOnlyList<EventSortOption> AvailableSortOptions { get; } =
    [
        EventSortOption.DateAscending,
        EventSortOption.DateDescending,
        EventSortOption.PriceAscending,
        EventSortOption.PriceDescending,
        EventSortOption.HistoricalRatingDescending,
    ];

    /// <summary>
    /// Creates a deep-enough copy for UI workflows that need to compare or edit state.
    /// </summary>
    /// <returns>A new instance of <see cref="EventListState"/>.</returns>
    public EventListState Clone()
    {
        return new EventListState
        {
            SearchText = this.SearchText,
            SelectedSortOption = this.SelectedSortOption,
            ActiveFilters = this.ActiveFilters.Clone(),
        };
    }

    /// <summary>
    /// Restores the search, sort, and filter state to the screen defaults.
    /// </summary>
    public void Reset()
    {
        this.SearchText = string.Empty;
        this.SelectedSortOption = EventSortOption.DateAscending;
        this.ActiveFilters.Reset();
    }
}
