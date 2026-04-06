using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class EventManagementViewModel : EventListPageViewModel
{
    private readonly IEventRepository? _eventRepository;
    private readonly INotificationService? _notificationService;

    public EventManagementViewModel()
    {
        _eventRepository = App.Services.EventRepository;
        _notificationService = App.Services.NotificationService;
        CreateEventCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(CreateEventAsync);
        EditEventCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(EditEventAsync, () => SelectedEvent is not null);
        DeleteEventCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(DeleteEventAsync, () => SelectedEvent is not null);
    }

    public override string PageTitle => "Event Management";

    public System.Windows.Input.ICommand SimulateUpdateCommand { get; }
    public System.Windows.Input.ICommand CreateEventCommand { get; }
    public System.Windows.Input.ICommand EditEventCommand { get; }
    public System.Windows.Input.ICommand DeleteEventCommand { get; }

    private Event? _selectedEvent;
    public Event? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            SetProperty(ref _selectedEvent, value);
            ((MovieApp.Ui.ViewModels.AsyncRelayCommand)EditEventCommand).NotifyCanExecuteChanged();
            ((MovieApp.Ui.ViewModels.AsyncRelayCommand)DeleteEventCommand).NotifyCanExecuteChanged();
        }
    }

    private string _formTitle = string.Empty;
    public string FormTitle { get => _formTitle; set => SetProperty(ref _formTitle, value); }

    private string _formLocation = string.Empty;
    public string FormLocation { get => _formLocation; set => SetProperty(ref _formLocation, value); }

    private string _formEventType = string.Empty;
    public string FormEventType { get => _formEventType; set => SetProperty(ref _formEventType, value); }

    private string _formDescription = string.Empty;
    public string FormDescription { get => _formDescription; set => SetProperty(ref _formDescription, value); }

    private DateTimeOffset? _formDate;
    public DateTimeOffset? FormDate { get => _formDate; set => SetProperty(ref _formDate, value); }

    private TimeSpan _formTime;
    public TimeSpan FormTime { get => _formTime; set => SetProperty(ref _formTime, value); }

    private double _formPrice;
    public double FormPrice { get => _formPrice; set => SetProperty(ref _formPrice, value); }

    private int _formCapacity;
    public int FormCapacity { get => _formCapacity; set => SetProperty(ref _formCapacity, value); }

    private string _formPosterUrl = string.Empty;
    public string FormPosterUrl { get => _formPosterUrl; set => SetProperty(ref _formPosterUrl, value); }

    private string _validationMessage = string.Empty;
    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (_eventRepository is null)
        {
            return [];
        }

        IEnumerable<Event> events = await _eventRepository.GetAllAsync();
        return events.ToList();
    }

    private void ClearForm()
    {
        FormTitle = string.Empty;
        FormLocation = string.Empty;
        FormEventType = string.Empty;
        FormDescription = string.Empty;
        FormDate = null;
        FormTime = TimeSpan.Zero;
        FormPrice = 0;
        FormCapacity = 0;
        FormPosterUrl = string.Empty;
        ValidationMessage = string.Empty;
        SelectedEvent = null;
    }

    private bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(FormTitle))
        {
            error = "Title cannot be empty.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(FormLocation))
        {
            error = "Location cannot be empty.";
            return false;
        }
        if (FormPrice < 0)
        {
            error = "Ticket price cannot be negative.";
            return false;
        }
        if (FormDate is null)
        {
            error = "Date is required.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private async Task CreateEventAsync()
    {
        if (_eventRepository is null) return;

        string error;
        if (!Validate(out error))
        {
            ValidationMessage = error;
            return;
        }

        ValidationMessage = string.Empty;
        DateTime date = FormDate!.Value.Date + FormTime;
        int currentUserId = App.Services.CurrentUserService?.CurrentUser.Id ?? 0;

        Event newEvent = new Event
        {
            Id = 0,
            Title = FormTitle.Trim(),
            Description = FormDescription,
            LocationReference = FormLocation.Trim(),
            TicketPrice = (decimal)FormPrice,
            EventDateTime = date,
            EventType = FormEventType != null ? FormEventType.Trim() : string.Empty,
            MaxCapacity = FormCapacity > 0 ? FormCapacity : 50,
            PosterUrl = FormPosterUrl,
            CreatorUserId = currentUserId,
        };

        await _eventRepository.AddAsync(newEvent);
        ClearForm();
        await InitializeAsync();
    }

    private async Task EditEventAsync()
    {
        if (_eventRepository is null || SelectedEvent is null) return;

        string error;
        if (!Validate(out error))
        {
            ValidationMessage = error;
            return;
        }

        ValidationMessage = string.Empty;
        DateTime date = FormDate!.Value.Date + FormTime;

        Event updated = new Event
        {
            Id = SelectedEvent.Id,
            Title = FormTitle.Trim(),
            Description = FormDescription,
            LocationReference = FormLocation.Trim(),
            TicketPrice = (decimal)FormPrice,
            EventDateTime = date,
            EventType = FormEventType != null ? FormEventType.Trim() : string.Empty,
            MaxCapacity = FormCapacity > 0 ? FormCapacity : SelectedEvent.MaxCapacity,
            PosterUrl = FormPosterUrl,
            CreatorUserId = SelectedEvent.CreatorUserId,
            CurrentEnrollment = SelectedEvent.CurrentEnrollment,
            HistoricalRating = SelectedEvent.HistoricalRating,
        };

        await _eventRepository.UpdateEventAsync(updated);
        ClearForm();
        await InitializeAsync();
    }

    private async Task DeleteEventAsync()
    {
        if (_eventRepository is null || SelectedEvent is null) return;
        await _eventRepository.DeleteAsync(SelectedEvent.Id);
        ClearForm();
        await InitializeAsync();
    }
}