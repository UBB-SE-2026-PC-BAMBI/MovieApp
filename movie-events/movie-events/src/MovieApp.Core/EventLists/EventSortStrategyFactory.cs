// <copyright file="EventSortStrategyFactory.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.EventLists;

using System;

/// <summary>
/// Factory to instantiate the correct sorting strategy based on the selected option.
/// </summary>
public static class EventSortStrategyFactory
{
    /// <summary>
    /// Returns the appropriate sorting strategy for the given sort option.
    /// </summary>
    /// <param name="sortOption">The selected sorting criteria.</param>
    /// <returns>An instance of <see cref="IEventSortStrategy"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid sort option is provided.</exception>
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