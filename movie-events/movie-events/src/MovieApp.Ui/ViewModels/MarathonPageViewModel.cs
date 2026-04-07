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
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Services;

/// <summary>
/// View model for the marathons page. Manages marathon selection, joining, leaderboards, and movie progress.
/// </summary>
public sealed class MarathonPageViewModel : ViewModelBase
{
    private readonly IMarathonService? marathonService;

    private int userId;
    private Marathon? selectedMarathon;
    private IReadOnlyList<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
    private MarathonProgress? currentProgress;
    private bool isLocked;
    private bool isJoined;
    private bool isLoading;
    private bool hasSelection;
    private bool isDataAvailable;

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
        this.marathonService = marathonService;
    }

    /// <summary>Gets the display items for the top card row.</summary>
    public ObservableCollection<MarathonDisplayItem> MarathonDisplayItems { get; } = new ();

    /// <summary>Gets the collection of available marathons.</summary>
    public ObservableCollection<Marathon> Marathons { get; } = new ();

    /// <summary>Gets a value indicating whether data is available from the service.</summary>
    public bool IsDataAvailable
    {
        get => this.isDataAvailable;
        private set
        {
            this.SetProperty(ref this.isDataAvailable, value);
            this.OnPropertyChanged(nameof(this.StatusVisibility));
        }
    }

    /// <summary>Gets the visibility of the status banner shown when no data is available.</summary>
    public Visibility StatusVisibility => this.IsDataAvailable
        ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Gets the currently selected marathon.</summary>
    public Marathon? SelectedMarathon
    {
        get => this.selectedMarathon;
        private set => this.SetProperty(ref this.selectedMarathon, value);
    }

    /// <summary>Gets the current leaderboard for the selected marathon.</summary>
    public IReadOnlyList<LeaderboardEntry> Leaderboard
    {
        get => this.leaderboard;
        private set => this.SetProperty(ref this.leaderboard, value);
    }

    /// <summary>Gets the current user's marathon progress.</summary>
    public MarathonProgress? CurrentProgress
    {
        get => this.currentProgress;
        private set
        {
            this.SetProperty(ref this.currentProgress, value);
            this.OnPropertyChanged(nameof(this.ProgressText));
        }
    }

    /// <summary>Gets a value indicating whether the selected marathon is locked for the current user.</summary>
    public bool IsLocked
    {
        get => this.isLocked;
        private set => this.SetProperty(ref this.isLocked, value);
    }

    /// <summary>Gets a value indicating whether the current user has joined the selected marathon.</summary>
    public bool IsJoined
    {
        get => this.isJoined;
        private set
        {
            this.SetProperty(ref this.isJoined, value);
            this.OnPropertyChanged(nameof(this.ShowJoinButton));
            this.OnPropertyChanged(nameof(this.ShowMovieList));
        }
    }

    /// <summary>Gets a value indicating whether data is currently being loaded.</summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set => this.SetProperty(ref this.isLoading, value);
    }

    /// <summary>Gets a value indicating whether a marathon has been selected.</summary>
    public bool HasSelection
    {
        get => this.hasSelection;
        private set => this.SetProperty(ref this.hasSelection, value);
    }

    /// <summary>Gets the list of movies in the selected marathon.</summary>
    public ObservableCollection<MarathonMovieItem> Movies { get; } = new ();

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
    /// <returns>A task that represents the asynchronous loading operation.</returns>
    public async Task LoadAsync(int userId)
    {
        this.userId = userId;

        if (this.marathonService is null)
        {
            this.IsDataAvailable = false;
            return;
        }

        this.IsDataAvailable = true;
        IEnumerable<Marathon> marathons = await this.marathonService.GetWeeklyMarathonsAsync(userId);

        this.Marathons.Clear();
        this.MarathonDisplayItems.Clear();
        DateTime weekEnd = GetSundayEnd();

        foreach (Marathon marathon in marathons)
        {
            this.Marathons.Add(marathon);

            int participantCount = await this.marathonService.GetParticipantCountAsync(marathon.Id);
            MarathonProgress? progress = await this.marathonService.GetUserProgressAsync(userId, marathon.Id);
            int totalMovies = await this.marathonService.GetMarathonMovieCountAsync(marathon.Id);

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
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SelectMarathonAsync(Marathon marathon)
    {
        this.SelectedMarathon = marathon;
        this.HasSelection = true;
        this.IsLoading = true;

        try
        {
            this.CurrentProgress = await this.marathonService!.GetCurrentProgressAsync(marathon.Id);
            this.IsJoined = this.CurrentProgress is not null;
            this.IsLocked = false;

            if (marathon.PrerequisiteMarathonId is int prereqId)
            {
                bool prereqDone = await this.marathonService!.IsPrerequisiteCompletedAsync(
                    this.userId,
                    prereqId);
                this.IsLocked = !prereqDone;
            }

            IEnumerable<LeaderboardEntry> leaderboard = await this.marathonService!.GetLeaderboardWithUsernamesAsync(marathon.Id);
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
        bool success = await this.marathonService!.StartMarathonAsync(marathonId);
        if (!success)
        {
            return false;
        }

        this.CurrentProgress = await this.marathonService!.GetCurrentProgressAsync(marathonId);
        this.IsJoined = true;
        await this.LoadMoviesAsync(marathonId);
        return true;
    }

    /// <summary>
    /// Refreshes the progress and leaderboard after a movie has been logged.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RefreshAfterMovieLoggedAsync()
    {
        int marathonId = this.SelectedMarathon!.Id;
        this.CurrentProgress = await this.marathonService!.GetCurrentProgressAsync(marathonId);

        IEnumerable<LeaderboardEntry> leaderboard =
            await this.marathonService!.GetLeaderboardWithUsernamesAsync(marathonId);
        this.Leaderboard = leaderboard.ToList();

        await this.LoadMoviesAsync(marathonId);
    }

    /// <summary>
    /// Logs the completion of a movie trivia session.
    /// </summary>
    /// <param name="marathonId">The marathon identifier.</param>
    /// <param name="movieId">The movie identifier.</param>
    /// <param name="correctAnswers">The number of correct answers.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LogMovieAsync(int marathonId, int movieId, int correctAnswers)
    {
        if (this.marathonService is null)
        {
            throw new InvalidOperationException("Marathon service is not available.");
        }

        await this.marathonService.LogMovieAsync(marathonId, movieId, correctAnswers);
    }

    /// <summary>
    /// Calculates the end of the current week (Sunday 23:59:59 UTC).
    /// </summary>
    /// <returns>The UTC date and time representing the end of the week.</returns>
    private static DateTime GetSundayEnd()
    {
        DateTime now = DateTime.UtcNow;
        int daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        DateTime monday = now.Date.AddDays(-daysFromMonday);
        return monday.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
    }

    /// <summary>
    /// Loads the movies associated with the specified marathon and updates their verification state.
    /// </summary>
    /// <param name="marathonId">The marathon identifier.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task LoadMoviesAsync(int marathonId)
    {
        if (this.marathonService is null)
        {
            throw new InvalidOperationException("Marathon service is not available.");
        }

        IEnumerable<Movie> movies = await this.marathonService.GetMoviesForMarathonAsync(marathonId);

        int verifiedCount = this.CurrentProgress?.CompletedMoviesCount ?? 0;
        this.Movies.Clear();
        IReadOnlyList<Movie> movieList = movies.ToList();

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
}
