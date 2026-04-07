using Moq;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class SlotMachineServiceTests
{
    private sealed class InMemoryStateRepo : IUserSlotMachineStateRepository
    {
        private readonly Dictionary<int, UserSpinData> _store = new();

        public Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(userId, out UserSpinData data);
            return Task.FromResult(data);
        }

        public Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default)
        {
            _store[state.UserId] = state;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default)
        {
            _store[state.UserId] = state;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryMovieRepo : IMovieRepository
    {
        public Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Genre>)new List<Genre> { new Genre { Id = 1, Name = "Action" } });
        }

        public Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Actor>)new List<Actor> { new Actor { Id = 2, Name = "Actor A" } });
        }

        public Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Director>)new List<Director> { new Director { Id = 3, Name = "Director A" } });
        }

        public Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
        {
            var movie = new Movie { Id = 10, Title = "Action Hit", Description = "", ReleaseYear = 2020, DurationMinutes = 120 };
            return Task.FromResult((IReadOnlyList<Movie>)new List<Movie> { movie });
        }

        public Task<IReadOnlyList<Movie>> FindMoviesByAnyCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
        {
            var movie = new Movie { Id = 10, Title = "Action Hit", Description = "", ReleaseYear = 2020, DurationMinutes = 120 };
            return Task.FromResult((IReadOnlyList<Movie>)new List<Movie> { movie });
        }

        public Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<int>)new List<int> { 100 });
        }

        public Task<IReadOnlyList<ReelCombination>> GetValidReelCombinationsAsync(CancellationToken cancellationToken = default)
        {
            ReelCombination combo = new ReelCombination
            {
                Genre = new Genre { Id = 1, Name = "Action" },
                Actor = new Actor { Id = 2, Name = "Actor A" },
                Director = new Director { Id = 3, Name = "Director A" }
            };
            return Task.FromResult((IReadOnlyList<ReelCombination>)new List<ReelCombination> { combo });
        }
    }

    private sealed class InMemoryEventRepo : IEventRepository
    {
        public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            DateTime future = DateTime.UtcNow.AddDays(2);
            return Task.FromResult((IEnumerable<Event>)new List<Event>
            {
                new Event { Id = 100, Title = "Action Hit Premiere", EventDateTime = future, PosterUrl = "", LocationReference = "Hall 1", TicketPrice = 15m, EventType = "Premiere", HistoricalRating = 4.5, CreatorUserId = 1 }
            });
        }

        public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateAsync(Event @event, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class InMemoryDiscountRepo : IUserMovieDiscountRepository
    {
        public readonly List<Reward> Rewards = new();

        public Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
        {
            Rewards.Add(reward);
            return Task.CompletedTask;
        }

        public Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Rewards.Where(result => result.OwnerUserId == userId).ToList());
        }

        public Task MarkRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task SpinAsync_HasAvailableSpins_ConsumesSpinAndGrantsRewardForJackpot()
    {
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        InMemoryMovieRepo movieRepo = new InMemoryMovieRepo();
        InMemoryEventRepo eventRepo = new InMemoryEventRepo();
        InMemoryDiscountRepo discountRepo = new InMemoryDiscountRepo();

        UserSpinData initialState = new UserSpinData { UserId = 1, DailySpinsRemaining = 1, BonusSpins = 0, LoginStreak = 3, EventSpinRewardsToday = 0, LastSlotSpinReset = DateTime.UtcNow };
        await stateRepo.CreateAsync(initialState);

        SlotMachineService service = new SlotMachineService(stateRepo, movieRepo, eventRepo, discountRepo);

        SlotMachineResult result = await service.SpinAsync(1);

        Assert.NotNull(result);
        Assert.Equal(0, (await stateRepo.GetByUserIdAsync(1))!.DailySpinsRemaining);
        Assert.True(result.JackpotDiscountApplied);
        Assert.Single(discountRepo.Rewards);
    }

    [Fact]
    public async Task SpinAsync_NoSpinsRemaining_ThrowsInvalidOperationException()
    {
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        InMemoryMovieRepo movieRepo = new InMemoryMovieRepo();
        InMemoryEventRepo eventRepo = new InMemoryEventRepo();
        InMemoryDiscountRepo discountRepo = new InMemoryDiscountRepo();

        UserSpinData initialState = new UserSpinData { UserId = 2, DailySpinsRemaining = 0, BonusSpins = 0, LoginStreak = 0, EventSpinRewardsToday = 0, LastSlotSpinReset = DateTime.UtcNow };
        await stateRepo.CreateAsync(initialState);

        SlotMachineService service = new SlotMachineService(stateRepo, movieRepo, eventRepo, discountRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SpinAsync(2));
    }

    [Fact]
    public async Task GrantBonusSpinForEventParticipationAsync_UnderDailyCap_GrantsOneSpin()
    {
        // SM.28 + SM.30: joining an event grants a bonus spin and the count reflects it immediately
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        InMemoryMovieRepo movieRepo = new InMemoryMovieRepo();
        InMemoryEventRepo eventRepo = new InMemoryEventRepo();
        InMemoryDiscountRepo discountRepo = new InMemoryDiscountRepo();

        UserSpinData state = new UserSpinData { UserId = 1, DailySpinsRemaining = 2, BonusSpins = 0, LoginStreak = 0, EventSpinRewardsToday = 0, LastSlotSpinReset = DateTime.UtcNow };
        await stateRepo.CreateAsync(state);

        SlotMachineService service = new SlotMachineService(stateRepo, movieRepo, eventRepo, discountRepo);

        bool granted = await service.GrantBonusSpinForEventParticipationAsync(1);
        Assert.True(granted);

        int spins = await service.GetAvailableSpinsAsync(1);
        Assert.Equal(3, spins); // 2 daily + 1 bonus
    }

    [Fact]
    public async Task GrantBonusSpinForEventParticipationAsync_ExceedsDailyCap_ReturnsFalse()
    {
        // SM.29: max 2 bonus spins from event participation per day
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        InMemoryMovieRepo movieRepo = new InMemoryMovieRepo();
        InMemoryEventRepo eventRepo = new InMemoryEventRepo();
        InMemoryDiscountRepo discountRepo = new InMemoryDiscountRepo();

        UserSpinData state = new UserSpinData { UserId = 1, DailySpinsRemaining = 5, BonusSpins = 0, LoginStreak = 0, EventSpinRewardsToday = 0, LastSlotSpinReset = DateTime.UtcNow };
        await stateRepo.CreateAsync(state);

        SlotMachineService service = new SlotMachineService(stateRepo, movieRepo, eventRepo, discountRepo);

        Assert.True(await service.GrantBonusSpinForEventParticipationAsync(1));  // 1st
        Assert.True(await service.GrantBonusSpinForEventParticipationAsync(1));  // 2nd
        Assert.False(await service.GrantBonusSpinForEventParticipationAsync(1)); // 3rd denied

        int spins = await service.GetAvailableSpinsAsync(1);
        Assert.Equal(7, spins); // 5 daily + 2 bonus (not 3)
    }

    [Fact]
    public async Task RecordLoginAndCheckStreakAsync_ThirdConsecutiveLogin_GrantsSpinAndResetsStreak()
    {
        // SM.32: three consecutive logins → bonus spin; SM.33: streak resets to 0
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        SlotMachineService service = new SlotMachineService(stateRepo, new InMemoryMovieRepo(), new InMemoryEventRepo(), new InMemoryDiscountRepo());

        // Simulate day-1 login
        UserSpinData state = new UserSpinData
        {
            UserId = 1,
            DailySpinsRemaining = 5,
            BonusSpins = 0,
            LoginStreak = 0,
            LastLoginDate = DateTime.UtcNow.Date.AddDays(-2),
            LastSlotSpinReset = DateTime.UtcNow,
        };
        await stateRepo.CreateAsync(state);

        // First login: streak goes 0→1, no spin yet
        Assert.False(await service.RecordLoginAndCheckStreakAsync(1));
        Assert.Equal(1, (await stateRepo.GetByUserIdAsync(1))!.LoginStreak);

        // Simulate next-day login
        (await stateRepo.GetByUserIdAsync(1))!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-1);
        await stateRepo.UpdateAsync((await stateRepo.GetByUserIdAsync(1))!);

        // Second login: streak goes 1→2, still no spin
        Assert.False(await service.RecordLoginAndCheckStreakAsync(1));
        Assert.Equal(2, (await stateRepo.GetByUserIdAsync(1))!.LoginStreak);

        // Simulate next-day login again
        (await stateRepo.GetByUserIdAsync(1))!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-1);
        await stateRepo.UpdateAsync((await stateRepo.GetByUserIdAsync(1))!);

        // Third login: streak reaches 3 → bonus spin granted, streak reset to 0
        Assert.True(await service.RecordLoginAndCheckStreakAsync(1));

        UserSpinData finalState = (await stateRepo.GetByUserIdAsync(1))!;
        Assert.Equal(0, finalState.LoginStreak);   // SM.33: streak reset
        Assert.Equal(1, finalState.BonusSpins);    // SM.32: spin awarded
    }

    [Fact]
    public async Task RecordLoginAndCheckStreakAsync_MissedPreviousDay_RestartsStreakAtOne()
    {
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        SlotMachineService service = new SlotMachineService(stateRepo, new InMemoryMovieRepo(), new InMemoryEventRepo(), new InMemoryDiscountRepo());

        // User had a streak of 2 but missed yesterday
        UserSpinData state = new UserSpinData
        {
            UserId = 1,
            DailySpinsRemaining = 5,
            BonusSpins = 0,
            LoginStreak = 2,
            LastLoginDate = DateTime.UtcNow.Date.AddDays(-3), // missed a day
            LastSlotSpinReset = DateTime.UtcNow,
        };
        await stateRepo.CreateAsync(state);

        Assert.False(await service.RecordLoginAndCheckStreakAsync(1));

        UserSpinData finalState = (await stateRepo.GetByUserIdAsync(1))!;
        Assert.Equal(1, finalState.LoginStreak);  // streak restarted
        Assert.Equal(0, finalState.BonusSpins);
    }

    [Fact]
    public async Task RecordLoginAndCheckStreakAsync_SameDayLogin_LeavesStreakUnchanged()
    {
        InMemoryStateRepo stateRepo = new InMemoryStateRepo();
        SlotMachineService service = new SlotMachineService(stateRepo, new InMemoryMovieRepo(), new InMemoryEventRepo(), new InMemoryDiscountRepo());

        UserSpinData state = new UserSpinData
        {
            UserId = 1,
            DailySpinsRemaining = 5,
            BonusSpins = 0,
            LoginStreak = 1,
            LastLoginDate = DateTime.UtcNow.Date, // already logged in today
            LastSlotSpinReset = DateTime.UtcNow,
        };
        await stateRepo.CreateAsync(state);

        // Calling again on the same day must not change the streak
        Assert.False(await service.RecordLoginAndCheckStreakAsync(1));
        Assert.Equal(1, (await stateRepo.GetByUserIdAsync(1))!.LoginStreak);
    }


    [Fact]
    public async Task SpinAsync_ThrowsInvalidOperationException_WhenNoValidCombinations()
    {
        var mockStateRepo = new Mock<IUserSlotMachineStateRepository>();
        var state = new UserSpinData { UserId = 1, DailySpinsRemaining = 1, LastSlotSpinReset = DateTime.UtcNow };
        mockStateRepo.Setup(r => r.GetByUserIdAsync(1, default)).ReturnsAsync(state);

        var mockMovieRepo = new Mock<IMovieRepository>();
        mockMovieRepo.Setup(r => r.GetValidReelCombinationsAsync(default)).ReturnsAsync(new List<ReelCombination>());

        var service = new SlotMachineService(mockStateRepo.Object, mockMovieRepo.Object, new Mock<IEventRepository>().Object, new Mock<IUserMovieDiscountRepository>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SpinAsync(1));
    }

    [Fact]
    public async Task GetAvailableSpinsAsync_ResetsSpins_WhenDateIsInPast()
    {
        var mockStateRepo = new Mock<IUserSlotMachineStateRepository>();
        var state = new UserSpinData { UserId = 1, DailySpinsRemaining = 0, LastSlotSpinReset = DateTime.UtcNow.AddDays(-1) };
        mockStateRepo.Setup(r => r.GetByUserIdAsync(1, default)).ReturnsAsync(state);

        var service = new SlotMachineService(mockStateRepo.Object, new Mock<IMovieRepository>().Object, new Mock<IEventRepository>().Object, new Mock<IUserMovieDiscountRepository>().Object);

        var spins = await service.GetAvailableSpinsAsync(1);

        Assert.Equal(5, spins); // Reset to default 5
        mockStateRepo.Verify(r => r.UpdateAsync(state, default), Times.Once);
    }

    [Fact]
    public async Task GrantStreakSpinAsync_ReturnsTrue_WhenStreakIsMet()
    {
        var mockStateRepo = new Mock<IUserSlotMachineStateRepository>();
        var state = new UserSpinData { UserId = 1, LoginStreak = 3, BonusSpins = 0 }; // 3 is RequiredLoginStreak
        mockStateRepo.Setup(r => r.GetByUserIdAsync(1, default)).ReturnsAsync(state);

        var service = new SlotMachineService(mockStateRepo.Object, new Mock<IMovieRepository>().Object, new Mock<IEventRepository>().Object, new Mock<IUserMovieDiscountRepository>().Object);

        var result = await service.GrantStreakSpinAsync(1);

        Assert.True(result);
        Assert.Equal(0, state.LoginStreak);
        Assert.Equal(1, state.BonusSpins);
    }

    [Fact]
    public async Task GrantStreakSpinAsync_ReturnsFalse_WhenStreakNotMet()
    {
        var mockStateRepo = new Mock<IUserSlotMachineStateRepository>();
        var state = new UserSpinData { UserId = 1, LoginStreak = 2, BonusSpins = 0 };
        mockStateRepo.Setup(r => r.GetByUserIdAsync(1, default)).ReturnsAsync(state);

        var service = new SlotMachineService(mockStateRepo.Object, new Mock<IMovieRepository>().Object, new Mock<IEventRepository>().Object, new Mock<IUserMovieDiscountRepository>().Object);

        var result = await service.GrantStreakSpinAsync(1);

        Assert.False(result);
        Assert.Equal(2, state.LoginStreak);
    }

    [Fact]
    public async Task WrapperMethods_ReturnExpectedValues()
    {
        // Tests GetGenresAsync, GetRandomGenreAsync, GetUserSpinStateAsync, GetMatchingEventsAsync
        var mockMovieRepo = new Mock<IMovieRepository>();
        var mockStateRepo = new Mock<IUserSlotMachineStateRepository>();
        var mockEventRepo = new Mock<IEventRepository>();

        mockMovieRepo.Setup(r => r.GetGenresAsync(default)).ReturnsAsync(new List<Genre> { new Genre { Id = 1, Name = "Test" } });
        mockMovieRepo.Setup(r => r.GetActorsAsync(default)).ReturnsAsync(new List<Actor> { new Actor { Id = 1, Name = "Test" } });
        mockMovieRepo.Setup(r => r.GetDirectorsAsync(default)).ReturnsAsync(new List<Director> { new Director { Id = 1, Name = "Test" } });

        var state = new UserSpinData { UserId = 1, LastSlotSpinReset = DateTime.UtcNow };
        mockStateRepo.Setup(r => r.GetByUserIdAsync(1, default)).ReturnsAsync(state);

        var service = new SlotMachineService(mockStateRepo.Object, mockMovieRepo.Object, mockEventRepo.Object, new Mock<IUserMovieDiscountRepository>().Object);

        var genres = await service.GetGenresAsync();
        var actors = await service.GetActorsAsync();
        var directors = await service.GetDirectorsAsync();
        var randomGenre = await service.GetRandomGenreAsync();
        var fetchedState = await service.GetUserSpinStateAsync(1);

        Assert.Single(genres);
        Assert.Single(actors);
        Assert.Single(directors);
        Assert.NotNull(randomGenre);
        Assert.Equal(1, fetchedState.UserId);
    }
}
