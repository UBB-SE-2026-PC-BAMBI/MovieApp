namespace MovieApp.Ui.ViewModels.Events;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;

/// <summary>
/// Manages event creation, editing, and deletion.
/// Notifies users of price drops and seat availability changes via <see cref="INotificationService"/>
/// directly, rather than through the Observer pattern, as a simple service call will suffice.
/// </summary>
public sealed class EventManagementViewModel : EventListPageViewModel
{
    private readonly IEventRepository? _eventRepository;
    private readonly INotificationService? _notificationService;

    public EventManagementViewModel()
    {
        _eventRepository = App.Services.EventRepository;
        _notificationService = App.Services.NotificationService;
        CreateEventCommand = new AsyncRelayCommand(CreateEventAsync);
        EditEventCommand = new AsyncRelayCommand(EditEventAsync, () => SelectedEvent is not null);
        DeleteEventCommand = new AsyncRelayCommand(DeleteEventAsync, () => SelectedEvent is not null);
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
            ((AsyncRelayCommand)EditEventCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteEventCommand).NotifyCanExecuteChanged();
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
        if (this._eventRepository is null)
        {
            return [];
        }

        IEnumerable<Event> events = await this._eventRepository.GetAllAsync();
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
        if (this._eventRepository is null)
        {
            return;
        }

        string error;
        if (!this.Validate(out error))
        {
            this.ValidationMessage = error;
            return;
        }

        this.ValidationMessage = string.Empty;
        DateTime date = this.FormDate!.Value.Date + this.FormTime;
        int currentUserId = App.Services.CurrentUserService?.CurrentUser.Id ?? 0;

        Event newEvent = new Event
        {
            Id = 0,
            Title = this.FormTitle.Trim(),
            Description = this.FormDescription,
            LocationReference = this.FormLocation.Trim(),
            TicketPrice = (decimal)this.FormPrice,
            EventDateTime = date,
            EventType = this.FormEventType != null ? this.FormEventType.Trim() : string.Empty,
            MaxCapacity = this.FormCapacity > 0 ? this.FormCapacity : 50,
            PosterUrl = this.FormPosterUrl,
            CreatorUserId = currentUserId,
        };

        await this._eventRepository.AddAsync(newEvent);
        this.ClearForm();
        await this.InitializeAsync();
    }

    private async Task EditEventAsync()
    {
        if (this._eventRepository is null || this.SelectedEvent is null)
        {
            return;
        }

        string error;
        if (!this.Validate(out error))
        {
            this.ValidationMessage = error;
            return;
        }

        this.ValidationMessage = string.Empty;
        DateTime date = this.FormDate!.Value.Date + this.FormTime;

        Event updated = new Event
        {
            Id = this.SelectedEvent.Id,
            Title = this.FormTitle.Trim(),
            Description = this.FormDescription,
            LocationReference = this.FormLocation.Trim(),
            TicketPrice = (decimal)this.FormPrice,
            EventDateTime = date,
            EventType = this.FormEventType != null ? this.FormEventType.Trim() : string.Empty,
            MaxCapacity = this.FormCapacity > 0 ? this.FormCapacity : this.SelectedEvent.MaxCapacity,
            PosterUrl = this.FormPosterUrl,
            CreatorUserId = this.SelectedEvent.CreatorUserId,
            CurrentEnrollment = this.SelectedEvent.CurrentEnrollment,
            HistoricalRating = this.SelectedEvent.HistoricalRating,
        };

        await this._eventRepository.UpdateEventAsync(updated);

        if (this._notificationService is not null)
        {
            if (updated.TicketPrice < this.SelectedEvent.TicketPrice)
            {
                await this._notificationService.NotifyPriceDropAsync(
                    updated.Id,
                    this.SelectedEvent.TicketPrice,
                    updated.TicketPrice);
            }

            if (updated.MaxCapacity > this.SelectedEvent.MaxCapacity)
            {
                await this._notificationService.NotifySeatsAvailableAsync(
                    updated.Id,
                    updated.MaxCapacity);
            }
        }

        this.ClearForm();
        await this.InitializeAsync();
    }

    private async Task DeleteEventAsync()
    {
        if (this._eventRepository is null || this.SelectedEvent is null)
        {
            return;
        }

        await this._eventRepository.DeleteAsync(this.SelectedEvent.Id);
        this.ClearForm();
        await this.InitializeAsync();
    }
}