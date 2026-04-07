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

        IEnumerable<Event> filteredEvents = ApplyFilters(events, state.ActiveFilters);
        IEnumerable<Event> searchedEvents = ApplySearch(filteredEvents, state.SearchText);
        IOrderedEnumerable<Event> sortedEvents = ApplySorting(searchedEvents, state.SelectedSortOption);

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

        string? eventType = string.IsNullOrWhiteSpace(filters.EventType) ? null : filters.EventType.Trim();
        string? locationReference = string.IsNullOrWhiteSpace(filters.LocationReference)
            ? null
            : filters.LocationReference.Trim();

        return events.Where(movieEvent =>
            (!filters.OnlyAvailableEvents || movieEvent.IsAvailable)
            && (eventType is null
                || string.Equals(movieEvent.EventType, eventType, StringComparison.OrdinalIgnoreCase))
            && (locationReference is null
                || string.Equals(movieEvent.LocationReference, locationReference, StringComparison.OrdinalIgnoreCase))
            && (!filters.MinimumTicketPrice.HasValue || movieEvent.TicketPrice >= filters.MinimumTicketPrice.Value)
            && (!filters.MaximumTicketPrice.HasValue || movieEvent.TicketPrice <= filters.MaximumTicketPrice.Value));
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

        string query = searchText.Trim();

        return events.Where(movieEvent =>
            movieEvent.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (movieEvent.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || movieEvent.LocationReference.Contains(query, StringComparison.OrdinalIgnoreCase)
            || movieEvent.EventType.Contains(query, StringComparison.OrdinalIgnoreCase));
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
        IEventSortStrategy strategy = EventSortStrategyFactory.GetStrategy(sortOption);
        return strategy.Sort(events);
    }
}
