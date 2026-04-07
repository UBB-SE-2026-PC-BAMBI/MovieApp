// <copyright file="SectionEventsViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Displays the event list for a single event section selected from the home page.
/// </summary>
/// <remarks>
/// The section is identified by <see cref="Context.GroupingValue"/>, which is matched
/// against the normalized <see cref="MovieApp.Core.Models.Event.EventType"/> of each event.
/// Events without a valid event type are ignored.
/// </remarks>
public sealed class SectionEventsViewModel(IEventRepository? repository, SectionNavigationContext context)
    : EventListPageViewModel
{
    private readonly IEventRepository? repository = repository;

    /// <summary>
    /// Gets the navigation context that defines which section this view model represents.
    /// </summary>
    public SectionNavigationContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets the page title shown for the selected section.
    /// </summary>
    public override string PageTitle => this.Context.Title;

    /// <summary>
    /// Gets a value indicating whether the selected section can load events from the database.
    /// </summary>
    public bool IsRepositoryAvailable => this.repository is not null;

    /// <summary>
    /// Gets the visibility of the repository-unavailable message.
    /// </summary>
    public Visibility UnavailableMessageVisibility => this.IsRepositoryAvailable
        ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the message shown when the selected section cannot load database-backed events.
    /// </summary>
    public string UnavailableMessage =>
        "Section events are unavailable because the database connection is not ready.";

    /// <summary>
    /// Loads all events for the current section and orders them by date.
    /// </summary>
    /// <returns>
    /// A task that resolves to the events whose normalized event type matches
    /// <see cref="SectionNavigationContext.GroupingValue"/>.
    /// </returns>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (this.repository is null)
        {
            return new List<Event>();
        }

        IEnumerable<Event> allEvents = await this.repository.GetAllAsync();

        return allEvents
            .Where(e => MatchesSection(e, this.Context.GroupingValue))
            .OrderBy(e => e.EventDateTime)
            .ToList();
    }

    /// <summary>
    /// Determines whether an event belongs to the currently selected section.
    /// </summary>
    /// <param name="event">The event being evaluated.</param>
    /// <param name="groupingValue">The normalized section grouping value to compare against.</param>
    /// <returns>
    /// <see langword="true"/> when the event has a non-empty event type that matches
    /// the supplied grouping value; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool MatchesSection(Event? @event, string groupingValue)
    {
        if (@event is null || string.IsNullOrWhiteSpace(@event.EventType) || string.IsNullOrWhiteSpace(groupingValue))
        {
            return false;
        }

        string? normalizedGroupingValue = groupingValue.Trim();
        string? eventGroupingValue = @event.EventType.Trim();

        return string.Equals(
            eventGroupingValue,
            normalizedGroupingValue,
            StringComparison.OrdinalIgnoreCase);
    }
}
