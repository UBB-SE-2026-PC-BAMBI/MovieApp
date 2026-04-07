// <copyright file="MarathonPageViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

/// <summary>
/// View model for the marathons page. Manages marathon selection, joining, leaderboards, and movie progress.
/// </summary>
public sealed class MarathonPageViewModel : ViewModelBase
{
    private readonly IMarathonService? _marathonService;

    private int _userId;
    private Marathon? _selectedMarathon;
    private IReadOnlyList<LeaderboardEntry> _leaderboard = [];
    private MarathonProgress? _currentProgress;
    private bool _isLocked;
    private bool _isJoined;
    private bool _isLoading;
    private bool _hasSelection;
    private bool _isDataAvailable;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarathonPageViewModel"/> class with no service.
    /// </summary>
    public MarathonPageViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarathonPageViewModel"/> class.
    /// </summary>
    /// <param name="marathonService">The marathon service.</param>
    public MarathonPageViewModel(IMarathonService marathonService)
    {
        _marathonService = marathonService;
    }

    /// <summary>Gets the display items for the top card row.</summary>
    public ObservableCollection<MarathonDisplayItem> MarathonDisplayItems { get; } = new();

    /// <summary>Gets the collection of available marathons.</summary>
    public ObservableCollection<Marathon> Marathons { get; } = new();

    /// <summary>Gets a value indicating whether data is available from the service.</summary>
    public bool IsDataAvailable
    {
        get => _isDataAvailable;
        private set
        {
            this.SetProperty(ref _isDataAvailable, value);
            this.OnPropertyChanged(nameof(this.StatusVisibility));
        }
    }

    /// <summary>Gets the visibility of the status banner shown when no data is available.</summary>
    public Visibility StatusVisibility => this.IsDataAvailable ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Gets or sets the currently selected marathon.</summary>
    public Marathon? SelectedMarathon
    {
        get => _selectedMarathon;
        private set => this.SetProperty(ref _selectedMarathon, value);
    }

    /// <summary>Gets the current leaderboard for the selected marathon.</summary>
    public IReadOnlyList<LeaderboardEntry> Leaderboard
    {
        get => _leaderboard;
        private set => this.SetProperty(ref _leaderboard, value);
    }

    /// <summary>Gets the current user's marathon progress.</summary>
    public MarathonProgress? CurrentProgress
    {
        get => _currentProgress;
        private set
        {
            this.SetProperty(ref _currentProgress, value);
            this.OnPropertyChanged(nameof(this.ProgressText));
        }
    }

    /// <summary>Gets a value indicating whether the selected marathon is locked for the current user.</summary>
    public bool IsLocked
    {
        get => _isLocked;
        private set => this.SetProperty(ref _isLocked, value);
    }

    /// <summary>Gets a value indicating whether the current user has joined the selected marathon.</summary>
    public bool IsJoined
    {
        get => _isJoined;
        private set
        {
            this.SetProperty(ref _isJoined, value);
            this.OnPropertyChanged(nameof(this.ShowJoinButton));
            this.OnPropertyChanged(nameof(this.ShowMovieList));
        }
    }

    /// <summary>Gets a value indicating whether data is currently being loaded.</summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => this.SetProperty(ref _isLoading, value);
    }

    /// <summary>Gets a value indicating whether a marathon has been selected.</summary>
    public bool HasSelection
    {
        get => _hasSelection;
        private set => this.SetProperty(ref _hasSelection, value);
    }

    /// <summary>Gets the list of movies in the selected marathon.</summary>
    public ObservableCollection<MarathonMovieItem> Movies { get; } = new();

    /// <summary>Gets a value indicating whether the join button should be shown.</summary>
    public bool ShowJoinButton => this.SelectedMarathon is not null && !this.IsJoined && !this.IsLocked;

    /// <summary>Gets a value indicating whether the movie list should be shown.</summary>
    public bool ShowMovieList => this.SelectedMarathon is not null && this.IsJoined;

    /// <summary>Gets a text summary of the user's current marathon progress.</summary>
    public string ProgressText => this.CurrentProgress is null
        ? "Not joined yet"
        : this.CurrentProgress.IsCompleted
            ? $"Completed — {this.CurrentProgress.CompletedMoviesCount} movies verified"
            : $"{this.CurrentProgress.CompletedMoviesCount} of {this.Movies.Count} movies verified";

    /// <summary>
    /// Loads the list of available marathons for the given user.
    /// </summary>
    /// <param name="userId">The current user's identifier.</param>
    public async Task LoadAsync(int userId)
    {
        _userId = userId;

        if (_marathonService is null)
        {
            this.IsDataAvailable = false;
            return;
        }

        this.IsDataAvailable = true;
        var marathons = await _marathonService.GetWeeklyMarathonsAsync(userId);

        this.Marathons.Clear();
        this.MarathonDisplayItems.Clear();
        var weekEnd = GetSundayEnd();

        foreach (var marathon in marathons)
        {
            this.Marathons.Add(marathon);

            var participantCount = await _marathonService.GetParticipantCountAsync(marathon.Id);
            var progress = await _marathonService.GetUserProgressAsync(userId, marathon.Id);
            var totalMovies = await _marathonService.GetMarathonMovieCountAsync(marathon.Id);

            this.MarathonDisplayItems.Add(new MarathonDisplayItem
            {
                Marathon = marathon,
                ParticipantCount = participantCount,
                UserAccuracy = progress?.TriviaAccuracy ?? 0,
                IsJoinedByUser = progress is not null,
                UserMoviesVerified = progress?.CompletedMoviesCount ?? 0,
                TotalMovies = totalMovies,
                WeekEnd = weekEnd,
            });
        }
    }

    /// <summary>
    /// Selects a marathon and loads its leaderboard and movie list.
    /// </summary>
    /// <param name="marathon">The marathon to select.</param>
    public async Task SelectMarathonAsync(Marathon marathon)
    {
        this.SelectedMarathon = marathon;
        this.HasSelection = true;
        this.IsLoading = true;

        try
        {
            this.CurrentProgress = await _marathonService!.GetCurrentProgressAsync(marathon.Id);
            this.IsJoined = this.CurrentProgress is not null;
            this.IsLocked = false;

            if (marathon.PrerequisiteMarathonId is int prereqId)
            {
                var prereqDone = await _marathonService!.IsPrerequisiteCompletedAsync(_userId, prereqId);
                this.IsLocked = !prereqDone;
            }

            var leaderboard = await _marathonService!.GetLeaderboardWithUsernamesAsync(marathon.Id);
            this.Leaderboard = leaderboard.ToList();

            if (this.IsJoined)
            {
                await this.LoadMoviesAsync(marathon.Id);
            }
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Joins the specified marathon for the current user.
    /// </summary>
    /// <param name="marathonId">The marathon identifier.</param>
    /// <returns><see langword="true"/> if the join succeeded; otherwise <see langword="false"/>.</returns>
    public async Task<bool> JoinMarathonAsync(int marathonId)
    {
        var success = await _marathonService!.StartMarathonAsync(marathonId);
        if (!success)
        {
            return false;
        }

        this.CurrentProgress = await _marathonService!.GetCurrentProgressAsync(marathonId);
        this.IsJoined = true;
        await this.LoadMoviesAsync(marathonId);
        return true;
    }

    /// <summary>
    /// Refreshes the progress and leaderboard after a movie has been logged.
    /// </summary>
    public async Task RefreshAfterMovieLoggedAsync()
    {
        var marathonId = this.SelectedMarathon!.Id;
        this.CurrentProgress = await _marathonService!.GetCurrentProgressAsync(marathonId);

        var leaderboard = await _marathonService!.GetLeaderboardWithUsernamesAsync(marathonId);
        this.Leaderboard = leaderboard.ToList();

        await this.LoadMoviesAsync(marathonId);
    }

    /// <summary>
    /// Logs the completion of a movie trivia session.
    /// </summary>
    /// <param name="marathonId">The marathon identifier.</param>
    /// <param name="movieId">The movie identifier.</param>
    /// <param name="correctAnswers">The number of correct answers.</param>
    public async Task LogMovieAsync(int marathonId, int movieId, int correctAnswers)
    {
        if (_marathonService is null)
        {
            throw new InvalidOperationException("Marathon service is not available.");
        }

        await _marathonService.LogMovieAsync(marathonId, movieId, correctAnswers);
    }

    private async Task LoadMoviesAsync(int marathonId)
    {
        var movies = await _marathonService.GetMoviesForMarathonAsync(marathonId);

        var verifiedCount = this.CurrentProgress?.CompletedMoviesCount ?? 0;
        this.Movies.Clear();
        var movieList = movies.ToList();

        for (int i = 0; i < movieList.Count; i++)
        {
            this.Movies.Add(new MarathonMovieItem
            {
                MovieId = movieList[i].Id,
                Title = movieList[i].Title,
                IsVerified = i < verifiedCount,
            });
        }

        this.OnPropertyChanged(nameof(this.ProgressText));
    }

    private static DateTime GetSundayEnd()
    {
        var now = DateTime.UtcNow;
        var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        var monday = now.Date.AddDays(-daysFromMonday);
        return monday.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
    }
}
