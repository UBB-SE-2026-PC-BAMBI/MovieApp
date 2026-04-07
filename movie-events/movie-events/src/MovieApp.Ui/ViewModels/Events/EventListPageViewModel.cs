// <copyright file="EventListPageViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using MovieApp.Core.EventLists;
using MovieApp.Core.Models;

/// <summary>
/// Base view model for screens that display a searchable, filterable,
/// and sortable event list.
/// </summary>
/// <remarks>
/// <see cref="AllEvents"/> stores the source data for the screen.
/// <br/>
/// <see cref="VisibleEvents"/> stores the transformed list shown in the UI.
/// <br/>
/// Call <see cref="RefreshVisibleEvents"/> after changing <see cref="EventListState"/>
/// or replacing <see cref="AllEvents"/>.
/// </remarks>
public abstract class EventListPageViewModel : ViewModelBase
{
    private IReadOnlyList<Event> allEvents = new List<Event>();
    private IReadOnlyList<Event> visibleEvents = new List<Event>();
    private bool isLoading;
    private bool hasNoEvents;

    /// <summary>
    /// Gets the title displayed for this page.
    /// </summary>
    public abstract string PageTitle { get; }

    /// <summary>
    /// Gets the current state used for searching, filtering, and sorting events.
    /// </summary>
    public EventListState EventListState { get; } = new ();

    /// <summary>
    /// Gets the available sort options for the current event list.
    /// </summary>
    public IReadOnlyList<EventSortOption> AvailableSortOptions => this.EventListState.AvailableSortOptions;

    /// <summary>
    /// Gets or sets full source event list owned by this screen.
    /// </summary>
    public IReadOnlyList<Event> AllEvents
    {
        get => this.allEvents;
        protected set => this.SetProperty(ref this.allEvents, value);
    }

    /// <summary>
    /// Gets or sets the transformed event list currently displayed by the UI.
    /// </summary>
    public IReadOnlyList<Event> VisibleEvents
    {
        get => this.visibleEvents;
        protected set => this.SetProperty(ref this.visibleEvents, value);
    }

    /// <summary>
    /// Gets a value indicating whether the event list is currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set
        {
            if (this.SetProperty(ref this.isLoading, value))
            {
                this.OnPropertyChanged(nameof(this.ShowEventList));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether no events exist after initialization completes.
    /// </summary>
    public bool HasNoEvents
    {
        get => this.hasNoEvents;
        private set
        {
            if (this.SetProperty(ref this.hasNoEvents, value))
            {
                this.OnPropertyChanged(nameof(this.ShowEventList));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether whether the event list should be shown in the UI.
    /// </summary>
    public bool ShowEventList => !this.IsLoading && !this.HasNoEvents;

    /// <summary>
    /// Asynchronously initializes the event list for this screen.
    /// </summary>
    /// <remarks>
    /// This method retrieves the screen's source events through
    /// <see cref="LoadEventsAsync"/>, assigns <see cref="AllEvents"/>,
    /// and rebuilds <see cref="VisibleEvents"/> using the current
    /// <see cref="EventListState"/>.
    /// <br/>
    /// Call this method before the page expects the event list to be displayed.
    /// </remarks>
    /// <returns>
    /// A task that represents the asynchronous initialization operation.
    /// </returns>
    public async Task InitializeAsync()
    {
        this.IsLoading = true;
        this.HasNoEvents = false;

        try
        {
            this.AllEvents = await this.LoadEventsAsync();
            this.RefreshVisibleEvents();
            this.HasNoEvents = this.AllEvents.Count == 0;
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Updates <see cref="EventListState.SearchText"/> and refreshes
    /// <see cref="VisibleEvents"/>.
    /// </summary>
    /// <param name="searchText">
    /// The new search text. A <see langword="null"/> value is treated as an empty string.
    /// </param>
    /// <remarks>
    /// If the effective value of <see cref="searchText"/> is not different from
    /// <see cref="EventListState.SearchText"/> the refresh will not be performed.
    /// </remarks>
    public void SetSearchText(string? searchText)
    {
        // Passing in a null value must allow the user to reset search.
        string normalizedSearchText = searchText ?? string.Empty;
        if (this.EventListState.SearchText == normalizedSearchText)
        {
            // Refresh not needed if the search text didn't change
            return;
        }

        this.EventListState.SearchText = normalizedSearchText;
        this.RefreshVisibleEvents();
    }

    /// <summary>
    /// Updates <see cref="EventListState.SelectedSortOption"/> and refreshes
    /// <see cref="VisibleEvents"/>.
    /// </summary>
    /// <param name="sortOption">
    /// The new sort option.
    /// </param>
    /// <remarks>
    /// If the effective value of <see cref="sortOption"/> is not different from
    /// <see cref="EventListState.SelectedSortOption"/> the refresh will not be performed.
    /// </remarks>
    public void SetSortOption(EventSortOption sortOption)
    {
        if (this.EventListState.SelectedSortOption == sortOption)
        {
            // Refresh not needed if the sort option did not change.
            return;
        }

        this.EventListState.SelectedSortOption = sortOption;
        this.RefreshVisibleEvents();
    }

    /// <summary>
    /// Applies a caller-provided update to <see cref="EventListState.ActiveFilters"/>
    /// and refreshes <see cref="VisibleEvents"/>.
    /// </summary>
    /// <param name="updateFilters">
    /// A delegate that mutates the current filter state for the current screen.
    /// </param>
    /// <remarks>
    /// Use this method to keep all filter changes centralized so the derived
    /// event list is always recomputed after filter state changes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="updateFilters"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateFilters(Action<EventFilterState> updateFilters)
    {
        ArgumentNullException.ThrowIfNull(updateFilters);

        updateFilters(this.EventListState.ActiveFilters);
        this.RefreshVisibleEvents();
    }

    /// <summary>
    /// Resets the <see cref="EventListState"/> to its default values and refreshes
    /// <see cref="VisibleEvents"/>.
    /// </summary>
    /// <remarks>
    /// This clears the current search text, selected sort option and active filters
    /// for the current screen.
    /// </remarks>
    public void ResetEventListState()
    {
        this.EventListState.Reset();
        this.RefreshVisibleEvents();
    }

    /// <summary>
    /// Rebuilds the visible event list by applying the current search, filter,
    /// and sort state to <see cref="AllEvents"/>.
    /// </summary>
    /// <remarks>
    /// This method must be called after any change to <see cref="EventListState"/>
    /// so that <see cref="VisibleEvents"/> raises a property change notification
    /// and the UI can refresh.
    /// </remarks>
    public void RefreshVisibleEvents()
    {
        this.VisibleEvents = EventListTransformer.Apply(this.AllEvents, this.EventListState);
    }

    /// <summary>
    /// Asynchronously loads the raw event list for this screen.
    /// </summary>
    /// <returns>
    /// A task that produces the source events used to populate <see cref="AllEvents"/>.
    /// </returns>
    /// <remarks>
    /// This method is responsible only for retrieving the source data for the screen.
    /// It should not update <see cref="VisibleEvents"/> directly.
    /// </remarks>
    protected abstract Task<IReadOnlyList<Event>> LoadEventsAsync();
}
