// <copyright file="EventSortStrategies.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.EventLists;

using System.Collections.Generic;
using System.Linq;
using MovieApp.Core.Models;

/// <summary>
/// Defines the contract for an event sorting strategy.
/// </summary>
public interface IEventSortStrategy
{
    /// <summary>
    /// Sorts the specified collection of events.
    /// </summary>
    /// <param name="events">The collection of events to sort.</param>
    /// <returns>An ordered sequence of events.</returns>
    IOrderedEnumerable<Event> Sort(IEnumerable<Event> events);
}

/// <summary>
/// Strategy for sorting events by date in ascending order.
/// </summary>
public sealed class DateAscendingSortStrategy : IEventSortStrategy
{
    /// <inheritdoc/>
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderBy(movieEvent => movieEvent.EventDateTime).ThenBy(movieEvent => movieEvent.Id);
}

/// <summary>
/// Strategy for sorting events by date in descending order.
/// </summary>
public sealed class DateDescendingSortStrategy : IEventSortStrategy
{
    /// <inheritdoc/>
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderByDescending(movieEvent => movieEvent.EventDateTime).ThenBy(movieEvent => movieEvent.Id);
}

/// <summary>
/// Strategy for sorting events by ticket price in ascending order.
/// </summary>
public sealed class PriceAscendingSortStrategy : IEventSortStrategy
{
    /// <inheritdoc/>
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderBy(movieEvent => movieEvent.TicketPrice).ThenBy(movieEvent => movieEvent.Id);
}

/// <summary>
/// Strategy for sorting events by ticket price in descending order.
/// </summary>
public sealed class PriceDescendingSortStrategy : IEventSortStrategy
{
    /// <inheritdoc/>
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderByDescending(movieEvent => movieEvent.TicketPrice).ThenBy(movieEvent => movieEvent.Id);
}

/// <summary>
/// Strategy for sorting events by historical rating in descending order.
/// </summary>
public sealed class HistoricalRatingDescendingSortStrategy : IEventSortStrategy
{
    /// <inheritdoc/>
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderByDescending(movieEvent => movieEvent.HistoricalRating).ThenBy(movieEvent => movieEvent.Id);
}