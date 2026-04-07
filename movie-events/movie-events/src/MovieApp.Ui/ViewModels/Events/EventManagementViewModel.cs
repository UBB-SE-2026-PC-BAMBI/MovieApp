// <copyright file="EventManagementViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;

/// <summary>
/// Manages event creation, editing, and deletion.
/// Notifies users of price drops and seat availability changes via <see cref="INotificationService"/>.
/// </summary>
public sealed class EventManagementViewModel : EventListPageViewModel
{
    /// <summary>
    /// Repository used for managing events.
    /// </summary>
    private readonly IEventRepository? eventRepository;

    /// <summary>
    /// Service used for sending notifications.
    /// </summary>
    private readonly INotificationService? notificationService;

    private Event? selectedEvent;
    private string formTitle = string.Empty;
    private string formLocation = string.Empty;
    private string formEventType = string.Empty;
    private string formDescription = string.Empty;
    private DateTimeOffset? formDate;
    private TimeSpan formTime;
    private double formPrice;
    private int formCapacity;
    private string formPosterUrl = string.Empty;
    private string validationMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventManagementViewModel"/> class.
    /// </summary>
    public EventManagementViewModel()
    {
        this.eventRepository = App.Services.EventRepository;
        this.notificationService = App.Services.NotificationService;
        this.CreateEventCommand = new AsyncRelayCommand(this.CreateEventAsync);
        this.EditEventCommand = new AsyncRelayCommand(this.EditEventAsync, () => this.SelectedEvent is not null);
        this.DeleteEventCommand = new AsyncRelayCommand(this.DeleteEventAsync, () => this.SelectedEvent is not null);
    }

    /// <summary>
    /// Gets the page title.
    /// </summary>
    public override string PageTitle => "Event Management";

    /// <summary>
    /// Gets the command for creating events.
    /// </summary>
    public System.Windows.Input.ICommand CreateEventCommand { get; }

    /// <summary>
    /// Gets the command for editing events.
    /// </summary>
    public System.Windows.Input.ICommand EditEventCommand { get; }

    /// <summary>
    /// Gets the command for deleting events.
    /// </summary>
    public System.Windows.Input.ICommand DeleteEventCommand { get; }

    /// <summary>
    /// Gets or sets the selected event.
    /// </summary>
    public Event? SelectedEvent
    {
        get => this.selectedEvent;
        set
        {
            this.SetProperty(ref this.selectedEvent, value);
            ((AsyncRelayCommand)this.EditEventCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)this.DeleteEventCommand).NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets or sets the event title input.
    /// </summary>
    public string FormTitle
    {
        get => this.formTitle;
        set => this.SetProperty(ref this.formTitle, value);
    }

    /// <summary>
    /// Gets or sets the event location input.
    /// </summary>
    public string FormLocation
    {
        get => this.formLocation;
        set => this.SetProperty(ref this.formLocation, value);
    }

    /// <summary>
    /// Gets or sets the event type input.
    /// </summary>
    public string FormEventType
    {
        get => this.formEventType;
        set => this.SetProperty(ref this.formEventType, value);
    }

    /// <summary>
    /// Gets or sets the event description input.
    /// </summary>
    public string FormDescription
    {
        get => this.formDescription;
        set => this.SetProperty(ref this.formDescription, value);
    }

    /// <summary>
    /// Gets or sets the event date input.
    /// </summary>
    public DateTimeOffset? FormDate
    {
        get => this.formDate;
        set => this.SetProperty(ref this.formDate, value);
    }

    /// <summary>
    /// Gets or sets the event time input.
    /// </summary>
    public TimeSpan FormTime
    {
        get => this.formTime;
        set => this.SetProperty(ref this.formTime, value);
    }

    /// <summary>
    /// Gets or sets the event price input.
    /// </summary>
    public double FormPrice
    {
        get => this.formPrice;
        set => this.SetProperty(ref this.formPrice, value);
    }

    /// <summary>
    /// Gets or sets the event capacity input.
    /// </summary>
    public int FormCapacity
    {
        get => this.formCapacity;
        set => this.SetProperty(ref this.formCapacity, value);
    }

    /// <summary>
    /// Gets or sets the event poster URL input.
    /// </summary>
    public string FormPosterUrl
    {
        get => this.formPosterUrl;
        set => this.SetProperty(ref this.formPosterUrl, value);
    }

    /// <summary>
    /// Gets the validation message.
    /// </summary>
    public string ValidationMessage
    {
        get => this.validationMessage;
        private set => this.SetProperty(ref this.validationMessage, value);
    }

    /// <inheritdoc/>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (this.eventRepository is null)
        {
            return new List<Event>();
        }

        IEnumerable<Event> events = await this.eventRepository.GetAllAsync();
        return events.ToList();
    }

    /// <summary>
    /// Clears the input form.
    /// </summary>
    private void ClearForm()
    {
        this.FormTitle = string.Empty;
        this.FormLocation = string.Empty;
        this.FormEventType = string.Empty;
        this.FormDescription = string.Empty;
        this.FormDate = null;
        this.FormTime = TimeSpan.Zero;
        this.FormPrice = 0;
        this.FormCapacity = 0;
        this.FormPosterUrl = string.Empty;
        this.ValidationMessage = string.Empty;
        this.SelectedEvent = null;
    }

    /// <summary>
    /// Validates the form inputs.
    /// </summary>
    /// <param name="error">The validation error message.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    private bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(this.FormTitle))
        {
            error = "Title cannot be empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.FormLocation))
        {
            error = "Location cannot be empty.";
            return false;
        }

        if (this.FormPrice < 0)
        {
            error = "Ticket price cannot be negative.";
            return false;
        }

        if (this.FormDate is null)
        {
            error = "Date is required.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CreateEventAsync()
    {
        if (this.eventRepository is null)
        {
            return;
        }

        if (!this.Validate(out string error))
        {
            this.ValidationMessage = error;
            return;
        }

        this.ValidationMessage = string.Empty;
        DateTime date = this.FormDate!.Value.Date + this.FormTime;
        int currentUserId = App.Services.CurrentUserService?.CurrentUser.Id ?? 0;

        Event newEvent = new ()
        {
            Id = 0,
            Title = this.FormTitle.Trim(),
            Description = this.FormDescription,
            LocationReference = this.FormLocation.Trim(),
            TicketPrice = (decimal)this.FormPrice,
            EventDateTime = date,
            EventType = this.FormEventType?.Trim() ?? string.Empty,
            MaxCapacity = this.FormCapacity > 0 ? this.FormCapacity : 50,
            PosterUrl = this.FormPosterUrl,
            CreatorUserId = currentUserId,
        };

        await this.eventRepository.AddAsync(newEvent);
        this.ClearForm();
        await this.InitializeAsync();
    }

    /// <summary>
    /// Edits the selected event.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EditEventAsync()
    {
        if (this.eventRepository is null || this.SelectedEvent is null)
        {
            return;
        }

        if (!this.Validate(out string error))
        {
            this.ValidationMessage = error;
            return;
        }

        this.ValidationMessage = string.Empty;
        DateTime date = this.FormDate!.Value.Date + this.FormTime;

        Event updated = new ()
        {
            Id = this.SelectedEvent.Id,
            Title = this.FormTitle.Trim(),
            Description = this.FormDescription,
            LocationReference = this.FormLocation.Trim(),
            TicketPrice = (decimal)this.FormPrice,
            EventDateTime = date,
            EventType = this.FormEventType?.Trim() ?? string.Empty,
            MaxCapacity = this.FormCapacity > 0 ? this.FormCapacity : this.SelectedEvent.MaxCapacity,
            PosterUrl = this.FormPosterUrl,
            CreatorUserId = this.SelectedEvent.CreatorUserId,
            CurrentEnrollment = this.SelectedEvent.CurrentEnrollment,
            HistoricalRating = this.SelectedEvent.HistoricalRating,
        };

        await this.eventRepository.UpdateEventAsync(updated);

        if (this.notificationService is not null)
        {
            if (updated.TicketPrice < this.SelectedEvent.TicketPrice)
            {
                await this.notificationService.NotifyPriceDropAsync(
                    updated.Id,
                    this.SelectedEvent.TicketPrice,
                    updated.TicketPrice);
            }

            if (updated.MaxCapacity > this.SelectedEvent.MaxCapacity)
            {
                await this.notificationService.NotifySeatsAvailableAsync(
                    updated.Id,
                    updated.MaxCapacity);
            }
        }

        this.ClearForm();
        await this.InitializeAsync();
    }

    /// <summary>
    /// Deletes the selected event.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task DeleteEventAsync()
    {
        if (this.eventRepository is null || this.SelectedEvent is null)
        {
            return;
        }

        await this.eventRepository.DeleteAsync(this.SelectedEvent.Id);
        this.ClearForm();
        await this.InitializeAsync();
    }
}