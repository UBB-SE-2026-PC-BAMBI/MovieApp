using System.Collections.Generic;
using System.Linq;
using MovieApp.Core.Models;

namespace MovieApp.Core.EventLists;

/// <summary>
/// Defines the contract for an event sorting strategy.
/// </summary>
public interface IEventSortStrategy
{
    IOrderedEnumerable<Event> Sort(IEnumerable<Event> events);
}

public sealed class DateAscendingSortStrategy : IEventSortStrategy
{
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderBy(e => e.EventDateTime).ThenBy(e => e.Id);
}

public sealed class DateDescendingSortStrategy : IEventSortStrategy
{
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderByDescending(e => e.EventDateTime).ThenBy(e => e.Id);
}

public sealed class PriceAscendingSortStrategy : IEventSortStrategy
{
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderBy(e => e.TicketPrice).ThenBy(e => e.Id);
}

public sealed class PriceDescendingSortStrategy : IEventSortStrategy
{
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderByDescending(e => e.TicketPrice).ThenBy(e => e.Id);
}

public sealed class HistoricalRatingDescendingSortStrategy : IEventSortStrategy
{
    public IOrderedEnumerable<Event> Sort(IEnumerable<Event> events) =>
        events.OrderByDescending(e => e.HistoricalRating).ThenBy(e => e.Id);
}