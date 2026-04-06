// <copyright file="EventSortOption.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.EventLists;

/// <summary>
/// Defines the available sorting options for event lists.
/// </summary>
public enum EventSortOption
{
    /// <summary>
    /// Sort by date in ascending order.
    /// </summary>
    DateAscending,

    /// <summary>
    /// Sort by date in descending order.
    /// </summary>
    DateDescending,

    /// <summary>
    /// Sort by price in ascending order.
    /// </summary>
    PriceAscending,

    /// <summary>
    /// Sort by price in descending order.
    /// </summary>
    PriceDescending,

    /// <summary>
    /// Sort by historical rating in descending order.
    /// </summary>
    HistoricalRatingDescending,
}
