using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Core.Tests.Fakes;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class MarathonServiceTests
{
    [Fact]
    public async Task GetWeeklyMarathonsAsync_UserHasNoAssignedMarathons_AssignsAndReturnsMarathons()
    {
        FakeMarathonRepository repository = new FakeMarathonRepository();
        MarathonService service = CreateService(repository);

        List<Marathon> result = (await service.GetWeeklyMarathonsAsync(userId: 10)).ToList();

        Assert.True(repository.AssignWeeklyMarathonsCalled);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StartMarathonAsync_PrerequisiteIsIncomplete_ReturnsFalse()
    {
        FakeMarathonRepository repository = new FakeMarathonRepository
        {
            ActiveMarathons =
            [
                new Marathon
                {
                    Id = 11,
                    Title = "Elite",
                    PrerequisiteMarathonId = 5,
                    IsActive = true,
                },
            ],
            IsPrerequisiteCompletedResult = false,
        };
        MarathonService service = CreateService(repository);

        bool result = await service.StartMarathonAsync(11);

        Assert.False(result);
        Assert.False(repository.JoinMarathonCalled);
    }

    [Fact]
    public async Task StartMarathonAsync_WhenUserAlreadyJoined_ReturnsTrueWithoutCallingJoin()
    {
        FakeMarathonRepository repository = new FakeMarathonRepository
        {
            ProgressByMarathonId =
            {
                [11] = new MarathonProgress { UserId = 10, MarathonId = 11 }
            }
        };
        MarathonService service = CreateService(repository);

        bool result = await service.StartMarathonAsync(11);

        Assert.True(result);
        Assert.False(repository.JoinMarathonCalled);
    }

    [Fact]
    public async Task UpdateQuizResultAsync_ValidQuizResult_UpdatesCompletedMoviesAndAccuracy()
    {
        MarathonProgress progress = new MarathonProgress
        {
            UserId = 10,
            MarathonId = 21,
            TriviaAccuracy = 50,
            CompletedMoviesCount = 1,
        };
        FakeMarathonRepository repository = new FakeMarathonRepository
        {
            ProgressByMarathonId = { [21] = progress },
        };
        MarathonService service = CreateService(repository);

        await service.UpdateQuizResultAsync(21, correctAnswers: 3);

        Assert.Equal(2, progress.CompletedMoviesCount);
        Assert.Equal(75, progress.TriviaAccuracy);
        Assert.Same(progress, repository.UpdatedProgress);
    }

    [Fact]
    public async Task LogMovieAsync_NotAllAnswersCorrect_ReturnsFalse()
    {
        FakeMarathonRepository repository = new FakeMarathonRepository
        {
            ProgressByMarathonId =
            {
                [21] = new MarathonProgress
                {
                    UserId = 10,
                    MarathonId = 21,
                    CompletedMoviesCount = 0,
                    TriviaAccuracy = 0,
                },
            },
            MovieCountsByMarathonId = { [21] = 2 },
        };
        MarathonService service = CreateService(repository);

        bool result = await service.LogMovieAsync(21, movieId: 100, correctAnswers: 2);

        Assert.False(result);
        Assert.Null(repository.UpdatedProgress);
    }

    [Fact]
    public async Task LogMovieAsync_WhenProgressNotFound_ReturnsFalse()
    {
        FakeMarathonRepository repository = new FakeMarathonRepository();
        MarathonService service = CreateService(repository);

        bool result = await service.LogMovieAsync(21, movieId: 100, correctAnswers: 3);

        Assert.False(result);
        Assert.Null(repository.UpdatedProgress);
    }

    [Fact]
    public async Task LogMovieAsync_WhenFirstMovieVerified_SetsAccuracyToFullScore()
    {
        MarathonProgress progress = new MarathonProgress
        {
            UserId = 10,
            MarathonId = 21,
            CompletedMoviesCount = 0,
            TriviaAccuracy = 0,
        };
        FakeMarathonRepository repository = new FakeMarathonRepository
        {
            ProgressByMarathonId = { [21] = progress },
            MovieCountsByMarathonId = { [21] = 5 },
        };
        MarathonService service = CreateService(repository);

        bool result = await service.LogMovieAsync(21, movieId: 100, correctAnswers: 3);

        Assert.True(result);
        Assert.Equal(1, progress.CompletedMoviesCount);
        Assert.Equal(100, progress.TriviaAccuracy);
        Assert.Same(progress, repository.UpdatedProgress);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public async Task LogMovieAsync_FinalMovieVerified_MarksMarathonAsCompleted()
    {
        MarathonProgress progress = new MarathonProgress
        {
            UserId = 10,
            MarathonId = 21,
            CompletedMoviesCount = 1,
            TriviaAccuracy = 100,
        };
        FakeMarathonRepository repository = new FakeMarathonRepository
        {
            ProgressByMarathonId = { [21] = progress },
            MovieCountsByMarathonId = { [21] = 2 },
        };
        MarathonService service = CreateService(repository);

        bool result = await service.LogMovieAsync(21, movieId: 100, correctAnswers: 3);

        Assert.True(result);
        Assert.Equal(2, progress.CompletedMoviesCount);
        Assert.True(progress.IsCompleted);
        Assert.NotNull(progress.FinishedAt);
    }

    private static MarathonService CreateService(FakeMarathonRepository repository)
    {
        return new MarathonService(repository, new StubCurrentUserService());
    }

    private sealed class StubCurrentUserService : ICurrentUserService
    {
        public User CurrentUser { get; } = new()
        {
            Id = 10,
            AuthProvider = "seed",
            AuthSubject = "dummy",
            Username = "alice",
        };

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}