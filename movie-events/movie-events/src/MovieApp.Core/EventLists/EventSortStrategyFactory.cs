using System;

namespace MovieApp.Core.EventLists;

/// <summary>
/// Factory to instantiate the correct sorting strategy based on the selected option.
/// </summary>
public static class EventSortStrategyFactory
{
    public static IEventSortStrategy GetStrategy(EventSortOption sortOption)
    {
        return sortOption switch
        {
            EventSortOption.DateAscending => new DateAscendingSortStrategy(),
            EventSortOption.DateDescending => new DateDescendingSortStrategy(),
            EventSortOption.PriceAscending => new PriceAscendingSortStrategy(),
            EventSortOption.PriceDescending => new PriceDescendingSortStrategy(),
            EventSortOption.HistoricalRatingDescending => new HistoricalRatingDescendingSortStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(sortOption), sortOption, null),
        };
    }
}