using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;


namespace MovieApp.Ui.Tests;

public sealed class MarathonPageViewModelTests
{
    [Fact]
    public async Task LoadAsync_ServiceHasWeeklyMarathons_PopulatesAvailableMarathons()
    {
        StubMarathonService service = new StubMarathonService
        {
            WeeklyMarathons =
            [
                new Marathon { Id = 1, Title = "One", IsActive = true },
                new Marathon { Id = 2, Title = "Two", IsActive = true },
            ],
        };
        MarathonPageViewModel viewModel = new MarathonPageViewModel(service);

        await viewModel.LoadAsync(userId: 10);

        Assert.Equal([1, 2], viewModel.Marathons.Select(m => m.Id));
    }

    [Fact]
    public async Task SelectMarathonAsync_ValidMarathon_LoadsProgressAndLeaderboard()
    {
        Marathon marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true };
        MarathonProgress progress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 1, TriviaAccuracy = 100 };
        StubMarathonService service = new StubMarathonService
        {
            Progress = progress,
            LeaderboardByMarathonId = { [7] = [progress] },
        };
        MarathonPageViewModel viewModel = new MarathonPageViewModel(service);

        await viewModel.SelectMarathonAsync(marathon);

        Assert.Same(marathon, viewModel.SelectedMarathon);
        Assert.Same(progress, viewModel.CurrentProgress);
        Assert.Equal([10], viewModel.Leaderboard.Select(entry => entry.UserId));
    }

    [Fact]
    public async Task RefreshAfterMovieLoggedAsync_MarathonSelected_ReloadsProgressAndLeaderboard()
    {
        Marathon marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true };
        MarathonProgress initialProgress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 1, TriviaAccuracy = 100 };
        MarathonProgress refreshedProgress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 2, TriviaAccuracy = 100, FinishedAt = DateTime.UtcNow };
        StubMarathonService service = new StubMarathonService
        {
            ProgressSequence = new Queue<MarathonProgress?>([initialProgress, refreshedProgress]),
            LeaderboardSequence = new Queue<IReadOnlyList<MarathonProgress>>([[initialProgress], [refreshedProgress]]),
        };
        MarathonPageViewModel viewModel = new MarathonPageViewModel(service);
        await viewModel.SelectMarathonAsync(marathon);

        await viewModel.RefreshAfterMovieLoggedAsync();

        Assert.Same(refreshedProgress, viewModel.CurrentProgress);
        Assert.Equal([2], viewModel.Leaderboard.Select(entry => entry.CompletedMoviesCount));
    }

    [Fact]
    public async Task SelectMarathonAsync_PrerequisiteNotCompleted_SetsIsLockedToTrue()
    {
        Marathon marathon = new Marathon
        {
            Id = 7,
            Title = "Elite",
            IsActive = true,
            PrerequisiteMarathonId = 3,
        };
        StubMarathonService service = new StubMarathonService
        {
            Progress = null,
            IsPrerequisiteCompleted = false
        };
        MarathonPageViewModel viewModel = new MarathonPageViewModel(service);

        await viewModel.SelectMarathonAsync(marathon);

        Assert.True(viewModel.IsLocked);
    }

    [Fact]
    public async Task LoadAsync_ServicesNotProvided_SetsUnavailableStateAndEmptyCollections()
    {
        MarathonPageViewModel viewModel = new MarathonPageViewModel();

        await viewModel.LoadAsync(userId: 10);

        Assert.False(viewModel.IsDataAvailable);
        Assert.Equal(Visibility.Visible, viewModel.StatusVisibility);
        Assert.Empty(viewModel.Marathons);
        Assert.Empty(viewModel.Leaderboard);
    }

    private sealed class StubMarathonService : IMarathonService
    {
        public IReadOnlyList<Marathon> WeeklyMarathons { get; set; } = [];
        public MarathonProgress? Progress { get; set; }
        public Queue<MarathonProgress?> ProgressSequence { get; set; } = new();
        public Dictionary<int, IReadOnlyList<MarathonProgress>> LeaderboardByMarathonId { get; } = [];
        public Queue<IReadOnlyList<MarathonProgress>> LeaderboardSequence { get; set; } = new();
        public bool IsPrerequisiteCompleted { get; set; } = true;

        public Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userId)
            => Task.FromResult<IEnumerable<Marathon>>(WeeklyMarathons);

        public Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId)
        {
            if (ProgressSequence.Count > 0)
                return Task.FromResult(ProgressSequence.Dequeue());
            return Task.FromResult(Progress);
        }

        public Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
            => Task.FromResult<MarathonProgress?>(null);

        public Task<bool> StartMarathonAsync(int marathonId)
            => Task.FromResult(true);

        public Task UpdateQuizResultAsync(int marathonId, int correctAnswers)
            => Task.CompletedTask;

        public Task<bool> LogMovieAsync(int marathonId, int movieId, int correctAnswers)
            => Task.FromResult(true);

        public Task<bool> JoinMarathonAsync(int userId, int marathonId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UpdateProgressAsync(MarathonProgress progress)
        {
            return Task.FromResult(true);
        }

        public Task<int> GetMarathonMovieCountAsync(int marathonId)
            => Task.FromResult(0);

        public Task<bool> IsPrerequisiteCompletedAsync(int userId, int marathonId)
            => Task.FromResult(IsPrerequisiteCompleted);

        public Task<IEnumerable<MovieApp.Core.Models.Movie.Movie>> GetMoviesForMarathonAsync(int marathonId)
            => Task.FromResult<IEnumerable<MovieApp.Core.Models.Movie.Movie>>([]);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int marathonId)
            => Task.FromResult<IEnumerable<LeaderboardEntry>>([]);

        public Task<int> GetParticipantCountAsync(int marathonId)
            => Task.FromResult(0);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId)
        {
            IEnumerable<MarathonProgress> source;

            if (LeaderboardSequence.Count > 0)
            {
                source = LeaderboardSequence.Dequeue();
            }
            else if (LeaderboardByMarathonId.TryGetValue(marathonId, out IReadOnlyList<MarathonProgress>? entries))
            {
                source = entries;
            }
            else
            {
                return Task.FromResult<IEnumerable<LeaderboardEntry>>([]);
            }

            return Task.FromResult<IEnumerable<LeaderboardEntry>>(source.Select(p => new LeaderboardEntry
            {
                UserId = p.UserId,
                Username = $"User{p.UserId}",
                CompletedMoviesCount = p.CompletedMoviesCount,
                TriviaAccuracy = p.TriviaAccuracy,
                FinishedAt = p.FinishedAt,
            }).ToList());
        }
    }
}