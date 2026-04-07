using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Services;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// ViewModel for the Slot Machine page.
/// Manages spin logic, reel display, and event results.
/// </summary>
public sealed class SlotMachineViewModel : ViewModelBase
{
    public event Action<Movie, int>? JackpotHit;

    private readonly ISlotMachineService _slotMachineService;
    private readonly ISlotMachineAnimationService _animationService;
    private readonly int _userId;

    private Genre _selectedGenre = new();
    private Actor _selectedActor = new();
    private Director _selectedDirector = new();
    private int _availableSpins;
    private int _bonusSpins;
    private int _loginStreak;
    private bool _isSpinning;
    private bool _isSpinButtonEnabled;
    private bool _isGenreSpinning;
    private bool _isActorSpinning;
    private bool _isDirectorSpinning;
    private string _statusMessage = "Ready to spin!";
    private Movie? _jackpotMovie;
    private bool _jackpotAchieved;
    private bool _hasMatchingEvents;
    private bool _hasNoMatchingEvents = true;

    public Genre SelectedGenre
    {
        get => _selectedGenre;
        private set => SetProperty(ref _selectedGenre, value);
    }

    public Actor SelectedActor
    {
        get => _selectedActor;
        private set => SetProperty(ref _selectedActor, value);
    }

    public Director SelectedDirector
    {
        get => _selectedDirector;
        private set => SetProperty(ref _selectedDirector, value);
    }

    public int AvailableSpins
    {
        get => _availableSpins;
        private set
        {
            if (SetProperty(ref _availableSpins, value))
                UpdateIsSpinButtonEnabled();
        }
    }

    public int BonusSpins
    {
        get => _bonusSpins;
        private set
        {
            if (SetProperty(ref _bonusSpins, value))
                UpdateIsSpinButtonEnabled();
        }
    }

    public int LoginStreak
    {
        get => _loginStreak;
        private set => SetProperty(ref _loginStreak, value);
    }

    public bool IsSpinning
    {
        get => _isSpinning;
        private set
        {
            if (SetProperty(ref _isSpinning, value))
                UpdateIsSpinButtonEnabled();
        }
    }

    public bool IsSpinButtonEnabled
    {
        get => _isSpinButtonEnabled;
        private set => SetProperty(ref _isSpinButtonEnabled, value);
    }

    private void UpdateIsSpinButtonEnabled()
    {
        IsSpinButtonEnabled = !IsSpinning && (AvailableSpins > 0 || BonusSpins > 0);
        _spinCommand?.NotifyCanExecuteChanged();
    }

    public bool IsGenreSpinning
    {
        get => _isGenreSpinning;
        private set => SetProperty(ref _isGenreSpinning, value);
    }

    public bool IsActorSpinning
    {
        get => _isActorSpinning;
        private set => SetProperty(ref _isActorSpinning, value);
    }

    public bool IsDirectorSpinning
    {
        get => _isDirectorSpinning;
        private set => SetProperty(ref _isDirectorSpinning, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<MatchingEventItem> MatchingEvents { get; } = new();

    public Movie? JackpotMovie
    {
        get => _jackpotMovie;
        private set => SetProperty(ref _jackpotMovie, value);
    }

    public bool JackpotAchieved
    {
        get => _jackpotAchieved;
        private set => SetProperty(ref _jackpotAchieved, value);
    }

    public bool HasMatchingEvents
    {
        get => _hasMatchingEvents;
        private set => SetProperty(ref _hasMatchingEvents, value);
    }

    public bool HasNoMatchingEvents
    {
        get => _hasNoMatchingEvents;
        private set => SetProperty(ref _hasNoMatchingEvents, value);
    }

    private AsyncRelayCommand? _spinCommand;

    public ICommand SpinCommand => _spinCommand ??= new AsyncRelayCommand(SpinAsync, CanSpin);

    public SlotMachineViewModel(
        int userId,
        ISlotMachineService slotMachineService,
        ISlotMachineAnimationService animationService)
    {
        _userId = userId;
        _slotMachineService = slotMachineService;
        _animationService = animationService;
        MatchingEvents.CollectionChanged += (_, _) =>
        {
            HasMatchingEvents = MatchingEvents.Count > 0;
            HasNoMatchingEvents = !HasMatchingEvents;
        };
    }

    public static SlotMachineViewModel CreateUnavailable(string statusMessage)
    {
        SlotMachineViewModel viewModel = new SlotMachineViewModel(0, null!, null!);

        viewModel.AvailableSpins = 0;
        viewModel.IsSpinButtonEnabled = false;
        viewModel.StatusMessage = statusMessage;

        return viewModel;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await LoadUserStateAsync(cancellationToken);
            UpdateIsSpinButtonEnabled();
            StatusMessage = App.StreakSpinGrantedOnLogin
                ? "3-day streak! Bonus spin awarded. Ready to spin!"
                : "Ready to spin!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error initializing: {ex.Message}";
        }
    }

    public async Task RefreshSpinCountAsync()
    {
        var state = await _slotMachineService.GetUserSpinStateAsync(_userId);
        AvailableSpins = state.DailySpinsRemaining;
        BonusSpins = state.BonusSpins;
        LoginStreak = state.LoginStreak;
    }

    private async Task LoadUserStateAsync(CancellationToken cancellationToken = default)
    {
        var state = await _slotMachineService.GetUserSpinStateAsync(_userId);
        AvailableSpins = state.DailySpinsRemaining;
        BonusSpins = state.BonusSpins;
        LoginStreak = state.LoginStreak;

        SelectedGenre = await _slotMachineService.GetRandomGenreAsync(cancellationToken);
        SelectedActor = await _slotMachineService.GetRandomActorAsync(cancellationToken);
        SelectedDirector = await _slotMachineService.GetRandomDirectorAsync(cancellationToken);
    }

    private bool CanSpin() => !IsSpinning && (AvailableSpins > 0 || BonusSpins > 0);

    private async Task SpinAsync()
    {
        if (IsSpinning || (AvailableSpins <= 0 && BonusSpins <= 0))
            return;

        IsSpinning = true;
        IsGenreSpinning = true;
        IsActorSpinning = true;
        IsDirectorSpinning = true;
        MatchingEvents.Clear();
        JackpotAchieved = false;
        StatusMessage = "Spinning...";

        try
        {
            var result = await _slotMachineService.SpinAsync(_userId);
            var genres = await _slotMachineService.GetGenresAsync();
            var actors = await _slotMachineService.GetActorsAsync();
            var directors = await _slotMachineService.GetDirectorsAsync();

            await _animationService.AnimateSpinAsync(
                result.Genre,
                result.Actor,
                result.Director,
                genres,
                actors,
                directors,
                genre => SelectedGenre = genre,
                actor => SelectedActor = actor,
                director => SelectedDirector = director,
                reelIndex =>
                {
                    switch (reelIndex)
                    {
                        case 0: IsGenreSpinning = false; break;
                        case 1: IsActorSpinning = false; break;
                        case 2: IsDirectorSpinning = false; break;
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
                StatusMessage = $"JACKPOT! {result.DiscountPercentage}% discount earned on {result.JackpotMovie?.Title}!";
                JackpotHit?.Invoke(result.JackpotMovie!, result.DiscountPercentage);
            }
            else if (MatchingEvents.Count > 0)
            {
                StatusMessage = $"Found {MatchingEvents.Count} matching events!";
            }
            else
            {
                StatusMessage = "No matching events this time. Try again!";
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
            StatusMessage = $"Error during spin: {ex.Message}";
        }
        finally
        {
            IsSpinning = false;
            ((AsyncRelayCommand)SpinCommand).NotifyCanExecuteChanged();
        }
    }
}

public sealed class MatchingEventItem
{
    public Event Event { get; }
    public bool IsJackpotEvent { get; }
    public string PriceText => EventCard.GetPriceText(Event, System.Globalization.CultureInfo.CurrentCulture);

    public MatchingEventItem(Event evt, bool isJackpotEvent)
    {
        Event = evt;
        IsJackpotEvent = isJackpotEvent;
    }
}

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Func<bool> _canExecute;

    public event EventHandler? CanExecuteChanged;

    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute ?? (() => true);
    }

    public bool CanExecute(object? parameter) => _canExecute();

    public async void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            await _executeAsync();
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}