// <copyright file="SlotMachineViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// ViewModel for the Slot Machine page.
/// Manages spin logic, reel display, and event results.
/// </summary>
public sealed class SlotMachineViewModel : ViewModelBase
{
    private readonly ISlotMachineService slotMachineService;
    private readonly ISlotMachineAnimationService animationService;
    private readonly int userId;

    private Genre selectedGenre = new ();
    private Actor selectedActor = new ();
    private Director selectedDirector = new ();
    private int availableSpins;
    private int bonusSpins;
    private int loginStreak;
    private bool isSpinning;
    private bool isSpinButtonEnabled;
    private bool isGenreSpinning;
    private bool isActorSpinning;
    private bool isDirectorSpinning;
    private string statusMessage = "Ready to spin!";

    private bool hasMatchingEvents;

    private bool hasNoMatchingEvents = true;

    private AsyncRelayCommand? spinCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotMachineViewModel"/> class.
    /// Creates a database-backed slot-machine view model for the current user.
    /// </summary>
    /// <param name="userId">The identifier of the current user.</param>
    /// <param name="slotMachineService">The slot machine service.</param>
    /// <param name="animationService">The animation service used for reel animations.</param>
    public SlotMachineViewModel(
        int userId,
        ISlotMachineService slotMachineService,
        ISlotMachineAnimationService animationService)
    {
        this.userId = userId;
        this.slotMachineService = slotMachineService;
        this.animationService = animationService;

        this.MatchingEvents.CollectionChanged += (_, _) =>
        {
            this.HasMatchingEvents = this.MatchingEvents.Count > 0;
            this.HasNoMatchingEvents = !this.HasMatchingEvents;
        };
    }

    /// <summary>
    /// Occurs when a jackpot is achieved during a spin.
    /// </summary>
    /// <remarks>
    /// Provides the winning movie and discount percentage.
    /// </remarks>
    public event Action<Movie, int>? JackpotHit;

    /// <summary>Gets the currently displayed genre on the reel.</summary>
    public Genre SelectedGenre
    {
        get => this.selectedGenre;
        private set => this.SetProperty(ref this.selectedGenre, value);
    }

    /// <summary>Gets the currently displayed actor on the reel.</summary>
    public Actor SelectedActor
    {
        get => this.selectedActor;
        private set => this.SetProperty(ref this.selectedActor, value);
    }

    /// <summary>Gets the currently displayed director on the reel.</summary>
    public Director SelectedDirector
    {
        get => this.selectedDirector;
        private set => this.SetProperty(ref this.selectedDirector, value);
    }

    /// <summary>Gets the number of remaining daily spins.</summary>
    public int AvailableSpins
    {
        get => this.availableSpins;
        private set
        {
            if (this.SetProperty(ref this.availableSpins, value))
            {
                this.UpdateIsSpinButtonEnabled();
            }
        }
    }

    /// <summary>Gets the number of bonus spins available.</summary>
    public int BonusSpins
    {
        get => this.bonusSpins;
        private set
        {
            if (this.SetProperty(ref this.bonusSpins, value))
            {
                this.UpdateIsSpinButtonEnabled();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether no matching events were found.
    /// </summary>
    public bool HasNoMatchingEvents
    {
        get => this.hasNoMatchingEvents;
        private set => this.SetProperty(ref this.hasNoMatchingEvents, value);
    }

    /// <summary>
    /// Gets a value indicating whether no matching events were found. Same fucking shit. Why am I doing this.
    /// </summary>
    public bool HasMatchingEvents
    {
        get => this.hasMatchingEvents;
        private set => this.SetProperty(ref this.hasMatchingEvents, value);
    }

    /// <summary>Gets the user's current login streak.</summary>
    public int LoginStreak
    {
        get => this.loginStreak;
        private set => this.SetProperty(ref this.loginStreak, value);
    }

    /// <summary>Gets a value indicating whether a spin is currently in progress.</summary>
    public bool IsSpinning
    {
        get => this.isSpinning;
        private set
        {
            if (this.SetProperty(ref this.isSpinning, value))
            {
                this.UpdateIsSpinButtonEnabled();
            }
        }
    }

    /// <summary>Gets a value indicating whether the spin button is enabled.</summary>
    public bool IsSpinButtonEnabled
    {
        get => this.isSpinButtonEnabled;
        private set => this.SetProperty(ref this.isSpinButtonEnabled, value);
    }

    /// <summary>Gets a value indicating whether the genre reel is spinning.</summary>
    public bool IsGenreSpinning
    {
        get => this.isGenreSpinning;
        private set => this.SetProperty(ref this.isGenreSpinning, value);
    }

    /// <summary>Gets a value indicating whether the actor reel is spinning.</summary>
    public bool IsActorSpinning
    {
        get => this.isActorSpinning;
        private set => this.SetProperty(ref this.isActorSpinning, value);
    }

    /// <summary>Gets a value indicating whether the director reel is spinning.</summary>
    public bool IsDirectorSpinning
    {
        get => this.isDirectorSpinning;
        private set => this.SetProperty(ref this.isDirectorSpinning, value);
    }

    /// <summary>Gets the current status message displayed to the user.</summary>
    public string StatusMessage
    {
        get => this.statusMessage;
        private set => this.SetProperty(ref this.statusMessage, value);
    }

    /// <summary>Gets the collection of events matching the current spin result.</summary>
    public ObservableCollection<MatchingEventItem> MatchingEvents { get; } = new ();

    /// <summary>Gets the movie associated with a jackpot result, if any.</summary>
    public Movie? JackpotMovie { get; private set; }

    /// <summary>Gets a value indicating whether a jackpot was achieved.</summary>
    public bool JackpotAchieved { get; private set; }

    /// <summary>Gets the command that triggers a slot machine spin.</summary>
    public ICommand SpinCommand => this.spinCommand ??= new AsyncRelayCommand(this.SpinAsync, this.CanSpin);

    /// <summary>
    /// Factory method that creates a <see cref="SlotMachineViewModel"/> in an unavailable state.
    /// </summary>
    /// <param name="statusMessage">The message to display when the slot machine is unavailable.</param>
    /// <returns>A <see cref="SlotMachineViewModel"/> with no spins and the spin button disabled.</returns>
    public static SlotMachineViewModel CreateUnavailable(string statusMessage)
    {
        SlotMachineViewModel viewModel = new SlotMachineViewModel(0, null!, null!);

        viewModel.AvailableSpins = 0;
        viewModel.IsSpinButtonEnabled = false;
        viewModel.StatusMessage = statusMessage;

        return viewModel;
    }

    /// <summary>
    /// Initializes the slot machine state for the current user.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the initialization.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await this.LoadUserStateAsync(cancellationToken);
            this.UpdateIsSpinButtonEnabled();

            this.StatusMessage = App.StreakSpinGrantedOnLogin
                ? "3-day streak! Bonus spin awarded. Ready to spin!"
                : "Ready to spin!";
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Error initializing: {ex.Message}";
        }
    }

    /// <summary>
    /// Lightweight refresh of the spin counter only.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RefreshSpinCountAsync()
    {
        UserSpinData? state = await this.slotMachineService.GetUserSpinStateAsync(this.userId);
        this.AvailableSpins = state.DailySpinsRemaining;
        this.BonusSpins = state.BonusSpins;
        this.LoginStreak = state.LoginStreak;
    }

    private async Task LoadUserStateAsync(CancellationToken cancellationToken = default)
    {
        UserSpinData? state = await this.slotMachineService.GetUserSpinStateAsync(this.userId);

        this.AvailableSpins = state.DailySpinsRemaining;
        this.BonusSpins = state.BonusSpins;
        this.LoginStreak = state.LoginStreak;

        this.SelectedGenre = await this.slotMachineService.GetRandomGenreAsync(cancellationToken);
        this.SelectedActor = await this.slotMachineService.GetRandomActorAsync(cancellationToken);
        this.SelectedDirector = await this.slotMachineService.GetRandomDirectorAsync(cancellationToken);
    }

    private bool CanSpin() => !this.IsSpinning && (this.AvailableSpins > 0 || this.BonusSpins > 0);

    /// <summary>
    /// Executes a slot machine spin.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    private async Task SpinAsync()
    {
        if (this.IsSpinning || (this.AvailableSpins <= 0 && this.BonusSpins <= 0))
        {
            return;
        }

        this.IsSpinning = true;
        this.IsGenreSpinning = true;
        this.IsActorSpinning = true;
        this.IsDirectorSpinning = true;
        this.MatchingEvents.Clear();
        this.JackpotAchieved = false;
        this.StatusMessage = "Spinning...";

        try
        {
            SlotMachineResult? result = await this.slotMachineService.SpinAsync(this.userId);

            IReadOnlyList<Genre> genres = await this.slotMachineService.GetGenresAsync();
            IReadOnlyList<Actor> actors = await this.slotMachineService.GetActorsAsync();
            IReadOnlyList<Director> directors = await this.slotMachineService.GetDirectorsAsync();

            await this.animationService.AnimateSpinAsync(
                result.Genre,
                result.Actor,
                result.Director,
                genres,
                actors,
                directors,
                g => this.SelectedGenre = g,
                a => this.SelectedActor = a,
                d => this.SelectedDirector = d,
                i =>
                {
                    if (i == 0)
                    {
                        this.IsGenreSpinning = false;
                    }
                    else if (i == 1)
                    {
                        this.IsActorSpinning = false;
                    }
                    else
                    {
                        this.IsDirectorSpinning = false;
                    }
                });

            JackpotMovie = result.JackpotMovie;
            JackpotAchieved = result.JackpotDiscountApplied;

            MatchingEvents.Clear();
            foreach (var evt in result.MatchingEvents)
            {
                var isJackpot = result.JackpotEventIds?.Contains(evt.Id) ?? false;
                MatchingEvents.Add(new MatchingEventItem(evt, isJackpot));
            }

            if (JackpotAchieved)
            {
                this.StatusMessage = $"JACKPOT! {result.DiscountPercentage}% discount earned on {result.JackpotMovie?.Title}!";
                this.JackpotHit?.Invoke(result.JackpotMovie!, result.DiscountPercentage);
            }
            else if (this.MatchingEvents.Count > 0)
            {
                this.StatusMessage = $"Found {this.MatchingEvents.Count} matching events!";
            }
            else
            {
                this.StatusMessage = "No matching events this time. Try again!";
            }

            var updatedState = await _slotMachineService.GetUserSpinStateAsync(_userId);
            AvailableSpins = updatedState.DailySpinsRemaining;
            BonusSpins = updatedState.BonusSpins;
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Error during spin: {ex.Message}";
        }
        finally
        {
            this.IsSpinning = false;
            ((AsyncRelayCommand)this.SpinCommand).NotifyCanExecuteChanged();
        }
    }

    private void UpdateIsSpinButtonEnabled()
    {
        this.IsSpinButtonEnabled = !this.IsSpinning && (this.AvailableSpins > 0 || this.BonusSpins > 0);
        this.spinCommand?.NotifyCanExecuteChanged();
    }
}

/// <summary>
/// Wraps an Event with a jackpot indicator for display.
/// </summary>
public sealed class MatchingEventItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MatchingEventItem"/> class.
    /// </summary>
    /// <param name="event">The associated event.</param>
    /// <param name="isJackpotEvent">Indicates whether the event is part of a jackpot.</param>
    public MatchingEventItem(Event @event, bool isJackpotEvent)
    {
        this.Event = @event;
        this.IsJackpotEvent = isJackpotEvent;
    }

    /// <summary>Gets the associated event.</summary>
    public Event Event { get; }

    /// <summary>Gets a value indicating whether the event is part of a jackpot.</summary>
    public bool IsJackpotEvent { get; }

    /// <summary>Gets the formatted price text.</summary>
    public string PriceText => EventCard.GetPriceText(
        this.Event,
        System.Globalization.CultureInfo.CurrentCulture);
}

/// <summary>
/// Simple async relay command implementation.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> executeAsync;
    private readonly Func<bool> canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
    /// </summary>
    /// <param name="executeAsync">The asynchronous action to execute.</param>
    /// <param name="canExecute">
    /// The predicate that determines whether the command can execute.
    /// If <see langword="null"/>, the command can always execute.
    /// </param>
    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        this.executeAsync = executeAsync;
        this.canExecute = canExecute ?? (() => true);
    }

    /// <summary>Occurs when the ability to execute changes.</summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns>
    /// <see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanExecute(object? parameter) => this.canExecute();

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public async void Execute(object? parameter)
    {
        if (this.CanExecute(parameter))
        {
            await this.executeAsync();
        }
    }

    /// <summary>Raises the <see cref="CanExecuteChanged"/> event.</summary>
    public void NotifyCanExecuteChanged()
    {
        this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}