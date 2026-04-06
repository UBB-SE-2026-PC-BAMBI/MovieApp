using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Navigation;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    private readonly IEventRepository? _repository;
    private IReadOnlyList<EventSection> _sections = [];

    public HomeEventsViewModel(IEventRepository? repository)
    {
        _repository = repository;
        PropertyChanged += OnBasePropertyChanged;
    }

    public override string PageTitle => "Home";

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

    public IReadOnlyList<EventSection> Sections
    {
        get => _sections;
        private set => SetProperty(ref _sections, value);
    }

    public bool IsRepositoryAvailable => _repository is not null;

    public Visibility UnavailableMessageVisibility => IsRepositoryAvailable ? Visibility.Collapsed : Visibility.Visible;

    public string UnavailableMessage => "Event data is unavailable because the database connection is not ready.";

    public SectionNavigationContext CreateNavigationContext(EventSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        return new SectionNavigationContext
        {
            Title = section.Title,
            GroupingValue = section.GroupingValue,
        };
    }

    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (_repository is null)
        {
            return [];
        }

        IEnumerable<Event> allEvents = await _repository.GetAllAsync();
        List<Event> eventsList = allEvents.ToList();

        if (App.EventUserStateService is not null)
        {
            foreach (Event evt in eventsList)
            {
                evt.DiscountPercentage = await App.EventUserStateService.GetDiscountForEventAsync(evt.Id);
                evt.IsJoined = await App.EventUserStateService.IsEventJoinedByUserAsync(evt.Id);
            }
        }

        return eventsList;
    }

    private void OnBasePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisibleEvents))
        {
            Sections = BuildSections(VisibleEvents);
        }
    }

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
}