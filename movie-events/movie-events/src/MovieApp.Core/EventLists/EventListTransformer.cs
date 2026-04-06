// <copyright file="EventListTransformer.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.EventLists;

using System;
using System.Collections.Generic;
using System.Linq;
using MovieApp.Core.Models;

/// <summary>
/// Provides transformation logic for event lists including filtering, searching, and sorting.
/// </summary>
public static class EventListTransformer
{
    /// <summary>
    /// Applies the full event-list pipeline in a stable order:
    /// filters first, then search, then sorting.
    /// </summary>
    /// <param name="events">The source collection of events.</param>
    /// <param name="state">The state containing filter, search, and sort criteria.</param>
    /// <returns>A transformed and materialized list of events.</returns>
    public static IReadOnlyList<Event> Apply(IEnumerable<Event> events, EventListState state)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(state);

        var filteredEvents = ApplyFilters(events, state.ActiveFilters);
        var searchedEvents = ApplySearch(filteredEvents, state.SearchText);
        var sortedEvents = ApplySorting(searchedEvents, state.SelectedSortOption);

        return sortedEvents.ToList();
    }

    /// <summary>
    /// Filters the event collection based on the provided criteria.
    /// </summary>
    /// <param name="events">The source events.</param>
    /// <param name="filters">The filter criteria to apply.</param>
    /// <returns>A filtered sequence of events.</returns>
    public static IEnumerable<Event> ApplyFilters(IEnumerable<Event> events, EventFilterState filters)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(filters);

        if (filters is { MinimumTicketPrice: not null, MaximumTicketPrice: not null }
            && filters.MinimumTicketPrice.Value > filters.MaximumTicketPrice.Value)
        {
            return [];
        }

        var eventType = string.IsNullOrWhiteSpace(filters.EventType) ? null : filters.EventType.Trim();
        var locationReference = string.IsNullOrWhiteSpace(filters.LocationReference)
            ? null
            : filters.LocationReference.Trim();

        return events.Where(e =>
            (!filters.OnlyAvailableEvents || e.IsAvailable)
            && (eventType is null
                || string.Equals(e.EventType, eventType, StringComparison.OrdinalIgnoreCase))
            && (locationReference is null
                || string.Equals(e.LocationReference, locationReference, StringComparison.OrdinalIgnoreCase))
            && (!filters.MinimumTicketPrice.HasValue || e.TicketPrice >= filters.MinimumTicketPrice.Value)
            && (!filters.MaximumTicketPrice.HasValue || e.TicketPrice <= filters.MaximumTicketPrice.Value));
    }

    /// <summary>
    /// Searches across the user-visible text fields for an event,
    /// including its title, description, location, and event type.
    /// </summary>
    /// <param name="events">The source events.</param>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>A sequence of events matching the search text.</returns>
    /// <remarks>
    /// This method is intentionally screen-agnostic. It filters only the sequence
    /// supplied by the caller, so different event-list screens can reuse the same
    /// search behavior without sharing state.
    /// </remarks>
    public static IEnumerable<Event> ApplySearch(IEnumerable<Event> events, string? searchText)
    {
        ArgumentNullException.ThrowIfNull(events);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return events;
        }

        var query = searchText.Trim();

        return events.Where(e =>
            e.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (e.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || e.LocationReference.Contains(query, StringComparison.OrdinalIgnoreCase)
            || e.EventType.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sorts the event collection based on the specified option.
    /// </summary>
    /// <param name="events">The source events.</param>
    /// <param name="sortOption">The sorting strategy to apply.</param>
    /// <returns>An ordered sequence of events.</returns>
    public static IOrderedEnumerable<Event> ApplySorting(IEnumerable<Event> events, EventSortOption sortOption)
    {
        ArgumentNullException.ThrowIfNull(events);

        // Strategy pattern implementation
        var strategy = EventSortStrategyFactory.GetStrategy(sortOption);
        return strategy.Sort(events);
    }
}
