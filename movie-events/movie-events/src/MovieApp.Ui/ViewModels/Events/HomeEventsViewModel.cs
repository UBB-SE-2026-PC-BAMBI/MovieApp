// <copyright file="HomeEventsViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Navigation;

/// <summary>
/// View model for the home screen, providing grouped event sections
/// and navigation shortcuts.
/// </summary>
public sealed class HomeEventsViewModel : EventListPageViewModel
{
    private readonly IEventRepository? repository;
    private IReadOnlyList<EventSection> sections = new List<EventSection>();

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeEventsViewModel"/> class.
    /// </summary>
    /// <param name="repository">The event repository used to load event data.</param>
    public HomeEventsViewModel(IEventRepository? repository)
    {
        this.repository = repository;
        this.PropertyChanged += this.OnBasePropertyChanged;
    }

    /// <inheritdoc/>
    public override string PageTitle => "Home";

    /// <summary>
    /// Gets the navigation shortcuts displayed on the home screen.
    /// </summary>
    public IReadOnlyList<HomeNavigationShortcut> NavigationShortcuts { get; } =
    [
        new HomeNavigationShortcut
        {
            Title = "My Events",
            Description = "Open your personal event workspace.",
            RouteTag = AppRouteResolver.MyEvents,
        },
        new HomeNavigationShortcut
        {
            Title = "Event Management",
            Description = "Open the event administration workspace.",
            RouteTag = AppRouteResolver.EventManagement,
        },
    ];

    /// <summary>
    /// Gets the grouped event sections displayed on the home screen.
    /// </summary>
    public IReadOnlyList<EventSection> Sections
    {
        get => this.sections;
        private set => this.SetProperty(ref this.sections, value);
    }

    /// <summary>
    /// Gets a value indicating whether the event repository is available.
    /// </summary>
    public bool IsRepositoryAvailable => this.repository is not null;

    /// <summary>
    /// Gets the visibility of the message shown when the repository is unavailable.
    /// </summary>
    public Visibility UnavailableMessageVisibility => this.IsRepositoryAvailable ?
        Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the message displayed when event data is unavailable.
    /// </summary>
    public string UnavailableMessage =>
        "Event data is unavailable because the database connection is not ready.";

    /// <summary>
    /// Creates a navigation context for the specified event section.
    /// </summary>
    /// <param name="section">The section used to build the navigation context.</param>
    /// <returns>The navigation context for the section.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="section"/> is <see langword="null"/>.
    /// </exception>
    public SectionNavigationContext CreateNavigationContext(EventSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        return new SectionNavigationContext
        {
            Title = section.Title,
            GroupingValue = section.GroupingValue,
        };
    }

    /// <summary>
    /// Loads all events from the repository and enriches them with user-specific state.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the list of events.
    /// </returns>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (this.repository is null)
        {
            return new List<Event>();
        }

        IEnumerable<Event> allEvents = await this.repository.GetAllAsync();
        List<Event> eventsList = allEvents.ToList();

        if (App.Services.EventUserStateService is not null)
        {
            foreach (Event @event in eventsList)
            {
                @event.DiscountPercentage =
                    await App.Services.EventUserStateService.GetDiscountForEventAsync(@event.Id);
                @event.IsJoined =
                    await App.Services.EventUserStateService.IsEventJoinedByUserAsync(@event.Id);
            }
        }

        return eventsList;
    }

    /// <summary>
    /// Groups events into sections based on their event type.
    /// </summary>
    /// <param name="events">The events to group.</param>
    /// <returns>A list of event sections grouped by event type.</returns>
    private static IReadOnlyList<EventSection> BuildSections(IEnumerable<Event> events)
    {
        Dictionary<string, EventSection> sectionsByType = new Dictionary<string, EventSection>(StringComparer.OrdinalIgnoreCase);

        foreach (Event @event in events.Where(e => !string.IsNullOrWhiteSpace(e.EventType)))
        {
            string eventType = @event.EventType.Trim();
            if (!sectionsByType.TryGetValue(eventType, out EventSection? section))
            {
                section = new EventSection
                {
                    Title = eventType,
                    GroupingValue = eventType,
                    Events = new List<Event>(),
                };
                sectionsByType[eventType] = section;
            }

            ((List<Event>)section.Events).Add(@event);
        }

        return sectionsByType.Values
            .OrderBy(section => section.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Handles property change notifications from the base class and updates sections when needed.
    /// </summary>
    /// <param name="sender">The source of the property change.</param>
    /// <param name="e">The event data describing the property change.</param>
    private void OnBasePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(this.VisibleEvents))
        {
            this.Sections = BuildSections(this.VisibleEvents);
        }
    }
}