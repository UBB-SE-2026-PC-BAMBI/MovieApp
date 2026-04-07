using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class MarathonPageViewModelTests
{
    [Fact]
    public async Task LoadAsync_PopulatesAvailableMarathons()
    {
        var service = new StubMarathonService
        {
            WeeklyMarathons =
            [
                new Marathon { Id = 1, Title = "One", IsActive = true },
                new Marathon { Id = 2, Title = "Two", IsActive = true },
            ],
        };
        var viewModel = new MarathonPageViewModel(service);

        await viewModel.LoadAsync(userId: 10);

        Assert.Equal([1, 2], viewModel.Marathons.Select(m => m.Id));
    }

    [Fact]
    public async Task SelectMarathonAsync_LoadsProgressAndLeaderboard()
    {
        var marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true };
        var progress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 1, TriviaAccuracy = 100 };
        var service = new StubMarathonService
        {
            Progress = progress,
            LeaderboardByMarathonId = { [7] = [progress] },
        };
        var viewModel = new MarathonPageViewModel(service);

        await viewModel.SelectMarathonAsync(marathon);

        Assert.Same(marathon, viewModel.SelectedMarathon);
        Assert.Same(progress, viewModel.CurrentProgress);
        Assert.Equal([10], viewModel.Leaderboard.Select(entry => entry.UserId));
    }

    [Fact]
    public async Task RefreshAfterMovieLoggedAsync_ReloadsProgressAndLeaderboardForSelectedMarathon()
    {
        var marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true };
        var initialProgress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 1, TriviaAccuracy = 100 };
        var refreshedProgress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 2, TriviaAccuracy = 100, FinishedAt = DateTime.UtcNow };
        var service = new StubMarathonService
        {
            ProgressSequence = new Queue<MarathonProgress?>([initialProgress, refreshedProgress]),
            LeaderboardSequence = new Queue<IReadOnlyList<MarathonProgress>>([[initialProgress], [refreshedProgress]]),
        };
        var viewModel = new MarathonPageViewModel(service);
        await viewModel.SelectMarathonAsync(marathon);

        await viewModel.RefreshAfterMovieLoggedAsync();

        Assert.Same(refreshedProgress, viewModel.CurrentProgress);
        Assert.Equal([2], viewModel.Leaderboard.Select(entry => entry.CompletedMoviesCount));
    }

    [Fact]
    public async Task SelectMarathonAsync_LocksEliteMarathonUntilItsPrerequisiteIsCompleted()
    {
        var marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true, PrerequisiteMarathonId = 3 };
        var service = new StubMarathonService
        {
            Progress = null,
            IsPrerequisiteCompleted = false,
        };
        var viewModel = new MarathonPageViewModel(service);

        await viewModel.SelectMarathonAsync(marathon);

        Assert.True(viewModel.IsLocked);
    }

    [Fact]
    public async Task LoadAsync_WithoutServices_ShowsUnavailableStateAndKeepsCollectionsEmpty()
    {
        var viewModel = new MarathonPageViewModel();

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

        public Task<int> GetParticipantCountAsync(int marathonId)
            => Task.FromResult(0);

        public Task<int> GetMarathonMovieCountAsync(int marathonId)
            => Task.FromResult(0);

        public Task<bool> IsPrerequisiteCompletedAsync(int userId, int marathonId)
            => Task.FromResult(IsPrerequisiteCompleted);

        public Task<IEnumerable<MovieApp.Core.Models.Movie.Movie>> GetMoviesForMarathonAsync(int marathonId)
            => Task.FromResult<IEnumerable<MovieApp.Core.Models.Movie.Movie>>([]);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int marathonId)
            => Task.FromResult<IEnumerable<LeaderboardEntry>>([]);

        public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId)
        {
            IEnumerable<MarathonProgress> source;

            if (LeaderboardSequence.Count > 0)
                source = LeaderboardSequence.Dequeue();
            else if (LeaderboardByMarathonId.TryGetValue(marathonId, out var entries))
                source = entries;
            else
                return Task.FromResult<IEnumerable<LeaderboardEntry>>([]);

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