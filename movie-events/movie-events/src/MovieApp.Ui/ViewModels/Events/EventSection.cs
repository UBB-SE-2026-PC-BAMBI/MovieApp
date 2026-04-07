// <copyright file="EventSection.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using MovieApp.Core.Models;

/// <summary>
/// Represents a logical home-page section containing events that share the same grouping value.
/// </summary>
/// <remarks>
/// The home page uses <see cref="Title"/> for display and <see cref="GroupingValue"/>
/// for navigation and filtering.
/// </remarks>
public sealed class EventSection
{
    /// <summary>
    /// Gets the display title shown for the section.
    /// </summary>
    required public string Title { get; init; }

    /// <summary>
    /// Gets the normalized grouping value used to reload this section on the detail page.
    /// </summary>
    required public string GroupingValue { get; init; }

    /// <summary>
    /// Gets the events that belong to this section.
    /// </summary>
    required public IReadOnlyList<Event> Events { get; init; }
}
