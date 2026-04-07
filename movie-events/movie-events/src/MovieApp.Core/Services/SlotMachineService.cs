// <copyright file="SlotMachineService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

/// <summary>
/// Orchestrates the slot machine logic, spins, and reward distribution.
/// </summary>
public sealed class SlotMachineService : ISlotMachineService
{
    private const int ResetSpinsCount = 5;
    private const int NoSpinsAvailable = 0;
    private const int DiscountPercentage = 70;
    private const double DiscountPercentageDouble = 70.0;
    private const int RequiredLoginStreak = 3;
    private const int MaximumEventSpinsPerDay = 2;

    private readonly IUserSlotMachineStateRepository stateRepository;
    private readonly IMovieRepository movieRepository;
    private readonly IEventRepository eventRepository;
    private readonly IUserMovieDiscountRepository discountRepository;
    private readonly Random random = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotMachineService"/> class.
    /// </summary>
    /// <param name="stateRepository">The spin state repository.</param>
    /// <param name="movieRepository">The movie metadata repository.</param>
    /// <param name="eventRepository">The event repository.</param>
    /// <param name="discountRepository">The discount reward repository.</param>
    public SlotMachineService(
        IUserSlotMachineStateRepository stateRepository,
        IMovieRepository movieRepository,
        IEventRepository eventRepository,
        IUserMovieDiscountRepository discountRepository)
    {
        this.stateRepository = stateRepository;
        this.movieRepository = movieRepository;
        this.eventRepository = eventRepository;
        this.discountRepository = discountRepository;
    }

    /// <inheritdoc/>
    public async Task<SlotMachineResult> SpinAsync(int userIdentifier)
    {
        UserSpinData? state = await this.stateRepository.GetByUserIdAsync(userIdentifier)
            ?? throw new InvalidOperationException("User state not found");

        DateTime currentUtcDate = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < currentUtcDate)
        {
            state.ResetDailySpins(ResetSpinsCount);
        }

        int totalSpinsCount = state.DailySpinsRemaining + state.BonusSpins;
        if (totalSpinsCount <= NoSpinsAvailable)
        {
            throw new InvalidOperationException("No available spins");
        }

        if (state.DailySpinsRemaining > NoSpinsAvailable)
        {
            state.DailySpinsRemaining--;
        }
        else
        {
            state.BonusSpins--;
        }

        IReadOnlyList<ReelCombination> validCombinations = await this.movieRepository.GetValidReelCombinationsAsync();
        if (validCombinations.Count == NoSpinsAvailable)
        {
            throw new InvalidOperationException("No movies with active screenings available");
        }

        List<Genre> distinctGenres = validCombinations.Select(combination => combination.Genre).DistinctBy(genre => genre.Id).ToList();
        List<Actor> distinctActors = validCombinations.Select(combination => combination.Actor).DistinctBy(actor => actor.Id).ToList();
        List<Director> distinctDirectors = validCombinations.Select(combination => combination.Director).DistinctBy(director => director.Id).ToList();

        Genre selectedGenre = distinctGenres[this.random.Next(distinctGenres.Count)];
        Actor selectedActor = distinctActors[this.random.Next(distinctActors.Count)];
        Director selectedDirector = distinctDirectors[this.random.Next(distinctDirectors.Count)];

        IReadOnlyList<Event> matchingEvents = await this.GetMatchingEventsAsync(selectedGenre.Id, selectedActor.Id, selectedDirector.Id);
        Movie? jackpotMovie = await this.FindJackpotMovieAsync(selectedGenre.Id, selectedActor.Id, selectedDirector.Id);

        HashSet<int> jackpotEventIdentifiers = new HashSet<int>();
        if (jackpotMovie is not null)
        {
            IReadOnlyList<int> eventIdentifiers = await this.movieRepository.FindScreeningEventIdsForMovieAsync(jackpotMovie.Id);
            foreach (int eventIdentifier in eventIdentifiers)
            {
                jackpotEventIdentifiers.Add(eventIdentifier);
            }
        }

        SlotMachineResult result = new SlotMachineResult
        {
            Genre = selectedGenre,
            Actor = selectedActor,
            Director = selectedDirector,
            MatchingEvents = matchingEvents.ToList(),
            JackpotEventIds = jackpotEventIdentifiers,
            JackpotMovie = jackpotMovie,
            JackpotDiscountApplied = false,
            DiscountPercentage = 0,
        };

        if (jackpotMovie is not null)
        {
            await this.GrantJackpotDiscount(userIdentifier, jackpotMovie.Id);
            result.JackpotDiscountApplied = true;
            result.DiscountPercentage = DiscountPercentage;
        }

        await this.stateRepository.UpdateAsync(state);
        return result;
    }

    /// <inheritdoc/>
    public async Task<int> GetAvailableSpinsAsync(int userIdentifier)
    {
        UserSpinData state = await this.stateRepository.GetByUserIdAsync(userIdentifier)
            ?? throw new InvalidOperationException("User state not found");

        DateTime currentUtcDate = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < currentUtcDate)
        {
            state.ResetDailySpins(ResetSpinsCount);
            await this.stateRepository.UpdateAsync(state);
        }

        return state.DailySpinsRemaining + state.BonusSpins;
    }

    /// <inheritdoc/>
    public async Task<UserSpinData> GetUserSpinStateAsync(int userIdentifier)
    {
        UserSpinData state = await this.stateRepository.GetByUserIdAsync(userIdentifier)
            ?? throw new InvalidOperationException("User state not found");

        DateTime currentUtcDate = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < currentUtcDate)
        {
            state.ResetDailySpins(ResetSpinsCount);
            await this.stateRepository.UpdateAsync(state);
        }

        return state;
    }

    /// <inheritdoc/>
    public async Task<bool> GrantBonusSpinForEventParticipationAsync(int userIdentifier)
    {
        UserSpinData state = await this.stateRepository.GetByUserIdAsync(userIdentifier)
            ?? throw new InvalidOperationException("User state not found");

        if (state.EventSpinRewardsToday < MaximumEventSpinsPerDay)
        {
            state.BonusSpins++;
            state.EventSpinRewardsToday++;
            await this.stateRepository.UpdateAsync(state);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> RecordLoginAndCheckStreakAsync(int userIdentifier)
    {
        UserSpinData state = await this.stateRepository.GetByUserIdAsync(userIdentifier)
            ?? throw new InvalidOperationException("User state not found");

        state.UpdateLoginStreak();

        bool isBonusGranted = false;
        if (state.LoginStreak >= RequiredLoginStreak)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            isBonusGranted = true;
        }

        await this.stateRepository.UpdateAsync(state);
        return isBonusGranted;
    }

    /// <inheritdoc/>
    public async Task<bool> GrantStreakSpinAsync(int userIdentifier)
    {
        UserSpinData state = await this.stateRepository.GetByUserIdAsync(userIdentifier)
            ?? throw new InvalidOperationException("User state not found");

        if (state.LoginStreak >= RequiredLoginStreak)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            await this.stateRepository.UpdateAsync(state);
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Genre> genres = await this.movieRepository.GetGenresAsync(cancellationToken);
        return genres[this.random.Next(genres.Count)];
    }

    /// <inheritdoc/>
    public async Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Actor> actors = await this.movieRepository.GetActorsAsync(cancellationToken);
        return actors[this.random.Next(actors.Count)];
    }

    /// <inheritdoc/>
    public async Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Director> directors = await this.movieRepository.GetDirectorsAsync(cancellationToken);
        return directors[this.random.Next(directors.Count)];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await this.movieRepository.GetGenresAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
    {
        return await this.movieRepository.GetActorsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
    {
        return await this.movieRepository.GetDirectorsAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier)
    {
        IReadOnlyList<Movie> matchingMovies = await this.movieRepository.FindMoviesByAnyCriteriaAsync(genreIdentifier, actorIdentifier, directorIdentifier);
        List<Event> resultEvents = new List<Event>();

        foreach (Movie movie in matchingMovies)
        {
            IReadOnlyList<int> eventIdentifiers = await this.movieRepository.FindScreeningEventIdsForMovieAsync(movie.Id);
            IEnumerable<Event> allEvents = await this.eventRepository.GetAllAsync();
            resultEvents.AddRange(allEvents.Where(eventEntity => eventIdentifiers.Contains(eventEntity.Id)
                && eventEntity.EventDateTime > DateTime.UtcNow));
        }

        return resultEvents.DistinctBy(eventEntity => eventEntity.Id).ToList();
    }

    /// <inheritdoc/>
    public async Task<Movie?> FindJackpotMovieAsync(int genreIdentifier, int actorIdentifier, int directorIdentifier)
    {
        IReadOnlyList<Movie> movies = await this.movieRepository.FindMoviesByCriteriaAsync(genreIdentifier, actorIdentifier, directorIdentifier);
        return movies.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task GrantJackpotDiscount(int userIdentifier, int movieIdentifier)
    {
        Reward jackpotReward = new Reward
        {
            RewardId = 0,
            RewardType = "MovieDiscount",
            RedemptionStatus = false,
            ApplicabilityScope = $"Movie:{movieIdentifier}",
            DiscountValue = DiscountPercentageDouble,
            OwnerUserId = userIdentifier,
            EventId = movieIdentifier,
        };

        await this.discountRepository.AddAsync(jackpotReward);
    }
}
