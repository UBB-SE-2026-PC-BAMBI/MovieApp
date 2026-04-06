// <copyright file="EventFilterState.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.EventLists;

/// <summary>
/// Mutable filter criteria applied to an event list screen.
/// </summary>
public sealed class EventFilterState
{
    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// Gets or sets the location reference.
    /// </summary>
    public string? LocationReference { get; set; }

    /// <summary>
    /// Gets or sets the minimum ticket price.
    /// </summary>
    public decimal? MinimumTicketPrice { get; set; }

    /// <summary>
    /// Gets or sets the maximum ticket price.
    /// </summary>
    public decimal? MaximumTicketPrice { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether only available events should be shown.
    /// </summary>
    public bool OnlyAvailableEvents { get; set; }

    /// <summary>
    /// Indicates whether any filter currently changes the visible event list.
    /// </summary>
    /// /// <returns>True if any filter is active; otherwise, false.</returns>
    public bool HasActiveFilters()
    {
        return !string.IsNullOrWhiteSpace(this.EventType)
            || !string.IsNullOrWhiteSpace(this.LocationReference)
            || this.MinimumTicketPrice is not null
            || this.MaximumTicketPrice is not null
            || this.OnlyAvailableEvents;
    }

    /// <summary>
    /// Creates a copy so callers can snapshot or branch filter state safely.
    /// </summary>
    /// /// <returns>A new instance of <see cref="EventFilterState"/> with copied values.</returns>
    public EventFilterState Clone()
    {
        return new EventFilterState
        {
            EventType = this.EventType,
            LocationReference = this.LocationReference,
            MinimumTicketPrice = this.MinimumTicketPrice,
            MaximumTicketPrice = this.MaximumTicketPrice,
            OnlyAvailableEvents = this.OnlyAvailableEvents,
        };
    }

    /// <summary>
    /// Restores all filters to their default unset values.
    /// </summary>
    public void Reset()
    {
        this.EventType = null;
        this.LocationReference = null;
        this.MinimumTicketPrice = null;
        this.MaximumTicketPrice = null;
        this.OnlyAvailableEvents = false;
    }
}
