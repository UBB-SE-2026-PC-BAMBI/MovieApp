using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

public sealed class SlotMachineService
{
    private readonly IUserSlotMachineStateRepository _stateRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserMovieDiscountRepository _discountRepository;
    private readonly Random _random = new();

    private const int RESET_SPINS = 5;
    private const int NO_SPINS = 0;
    private const int DISCOUNT_PERCENTAGE = 70;
    private const double DISCOUNT_PERCENTAGE_DOUBLE = 70.0;
    private const int LOGIN_STREAK = 3;
    private const int MAX_EVENT_SPIN_PER_DAY = 2;
    public SlotMachineService(
        IUserSlotMachineStateRepository stateRepository,
        IMovieRepository movieRepository,
        IEventRepository eventRepository,
        IUserMovieDiscountRepository discountRepository)
    {
        _stateRepository = stateRepository;
        _movieRepository = movieRepository;
        _eventRepository = eventRepository;
        _discountRepository = discountRepository;
    }

    public async Task<SlotMachineResult> SpinAsync(int userId)
    {
        UserSpinData? state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        DateTime today = DateTime.UtcNow.Date;
        DateTime lastReset = state.LastSlotSpinReset.Date;
        if (lastReset < today)
        {
            state.ResetDailySpins(RESET_SPINS);
        }

        int totalSpins = state.DailySpinsRemaining + state.BonusSpins;
        if (totalSpins <= NO_SPINS)
            throw new InvalidOperationException("No available spins");

        if (state.DailySpinsRemaining > NO_SPINS)
            state.DailySpinsRemaining--;
        else
            state.BonusSpins--;

        IReadOnlyList<ReelCombination> validCombinations = await _movieRepository.GetValidReelCombinationsAsync();
        if (validCombinations.Count == NO_SPINS)
            throw new InvalidOperationException("No movies with active screenings available");

        List<Genre> distinctGenres = validCombinations.Select(c => c.Genre).DistinctBy(g => g.Id).ToList();
        List<Actor> distinctActors = validCombinations.Select(c => c.Actor).DistinctBy(a => a.Id).ToList();
        List<Director> distinctDirectors = validCombinations.Select(c => c.Director).DistinctBy(d => d.Id).ToList();

        Genre genre = distinctGenres[_random.Next(distinctGenres.Count)];
        Actor actor = distinctActors[_random.Next(distinctActors.Count)];
        Director director = distinctDirectors[_random.Next(distinctDirectors.Count)];

        IReadOnlyList<Event> matchingEvents = await GetMatchingEventsAsync(genre.Id, actor.Id, director.Id);
        Movie? jackpotMovie = await FindJackpotMovieAsync(genre.Id, actor.Id, director.Id);

        HashSet<int> jackpotEventIds = new HashSet<int>();
        if (jackpotMovie is not null)
        {
            IReadOnlyList<int> jpEventIds = await _movieRepository.FindScreeningEventIdsForMovieAsync(jackpotMovie.Id);
            foreach (int id in jpEventIds)
                jackpotEventIds.Add(id);
        }

        SlotMachineResult result = new SlotMachineResult
        {
            Genre                  = genre,
            Actor                  = actor,
            Director               = director,
            MatchingEvents         = matchingEvents.ToList(),
            JackpotEventIds        = jackpotEventIds,
            JackpotMovie           = jackpotMovie,
            JackpotDiscountApplied = false,
            DiscountPercentage     = 0
        };

        if (jackpotMovie is not null)
        {
            await GrantJackpotDiscount(userId, jackpotMovie.Id);
            result.JackpotDiscountApplied = true;
            result.DiscountPercentage = DISCOUNT_PERCENTAGE;
        }

        await _stateRepository.UpdateAsync(state);

        return result;
    }

    public async Task<int> GetAvailableSpinsAsync(int userId)
    {
        UserSpinData state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        DateTime today = DateTime.UtcNow.Date;
        DateTime lastReset = state.LastSlotSpinReset.Date;
        if (lastReset < today)
        {
            state.ResetDailySpins(RESET_SPINS); 
            await _stateRepository.UpdateAsync(state); 
        }

        return state.DailySpinsRemaining + state.BonusSpins;
    }

    /// <summary>
    /// Returns the full spin state for a user (daily spins, bonus spins, login streak),
    /// resetting daily spins if the day has rolled over.
    /// </summary>
    public async Task<UserSpinData> GetUserSpinStateAsync(int userId)
    {
        UserSpinData state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        DateTime today = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < today)
        {
            state.ResetDailySpins(RESET_SPINS);
            await _stateRepository.UpdateAsync(state);
        }

        return state;
    }

    public async Task<bool> GrantBonusSpinForEventParticipationAsync(int userId)
    {
        UserSpinData state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");
        if (state.EventSpinRewardsToday < MAX_EVENT_SPIN_PER_DAY)
        {
            state.BonusSpins++;
            state.EventSpinRewardsToday++;
            await _stateRepository.UpdateAsync(state);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Records the current login against the user's streak counter and, if a
    /// three-day consecutive streak is reached (SM.32), grants one bonus spin
    /// and resets the streak counter (SM.33).
    /// Safe to call once per app session; calling more than once on the same day
    /// is idempotent because <see cref="UserSpinData.UpdateLoginStreak"/> only
    /// increments when the last login was on the previous calendar day.
    /// </summary>
    /// <returns><c>true</c> if a streak bonus spin was awarded.</returns>
    public async Task<bool> RecordLoginAndCheckStreakAsync(int userId)
    {
        UserSpinData state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        state.UpdateLoginStreak();

        bool granted = false;
        if (state.LoginStreak >= LOGIN_STREAK)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            granted = true;
        }

        await _stateRepository.UpdateAsync(state);
        return granted;
    }

    public async Task<bool> GrantStreakSpinAsync(int userId)
    {
        UserSpinData state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");
        if (state.LoginStreak >= LOGIN_STREAK)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            await _stateRepository.UpdateAsync(state);
            return true;
        }
        return false;
    }

    public async Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Genre> genres = await _movieRepository.GetGenresAsync(cancellationToken);
        return genres[_random.Next(genres.Count)];
    }

    public async Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Actor> actors = await _movieRepository.GetActorsAsync(cancellationToken);
        return actors[_random.Next(actors.Count)];
    }

    public async Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Director> directors = await _movieRepository.GetDirectorsAsync(cancellationToken);
        return directors[_random.Next(directors.Count)];
    }

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetGenresAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetActorsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetDirectorsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreId, int actorId, int directorId)
    {
        IReadOnlyList<Movie> movies = await _movieRepository.FindMoviesByAnyCriteriaAsync(genreId, actorId, directorId);
        List<Event> events = new List<Event>();
        foreach (Movie movie in movies)
        {
            IReadOnlyList<int> eventIds = await _movieRepository.FindScreeningEventIdsForMovieAsync(movie.Id);
            IEnumerable<Event> allEvents = await _eventRepository.GetAllAsync();
            events.AddRange(allEvents.Where(e => eventIds.Contains(e.Id) && e.EventDateTime > DateTime.UtcNow));
        }

        return events.DistinctBy(e => e.Id).ToList();
    }

    public async Task<Movie?> FindJackpotMovieAsync(int genreId, int actorId, int directorId)
    {
        IReadOnlyList<Movie> movies = await _movieRepository.FindMoviesByCriteriaAsync(genreId, actorId, directorId);
        return movies.FirstOrDefault();
    }

    public async Task GrantJackpotDiscount(int userId, int movieId)
    {
        Reward reward = new Reward
        {
            RewardId           = 0,
            RewardType         = "MovieDiscount",
            RedemptionStatus   = false,
            ApplicabilityScope = $"Movie:{movieId}",
            DiscountValue      = DISCOUNT_PERCENTAGE_DOUBLE,
            OwnerUserId        = userId,
            EventId            = movieId
        };

        await _discountRepository.AddAsync(reward);
    }
}
