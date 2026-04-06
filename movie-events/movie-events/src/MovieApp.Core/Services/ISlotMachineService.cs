using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;

namespace MovieApp.Core.Services;

public interface ISlotMachineService
{
    Task<SlotMachineResult> SpinAsync(int userId);
    Task<int> GetAvailableSpinsAsync(int userId);
    Task<UserSpinData> GetUserSpinStateAsync(int userId);
    Task<bool> GrantBonusSpinForEventParticipationAsync(int userId);
    Task<bool> RecordLoginAndCheckStreakAsync(int userId);
    Task<bool> GrantStreakSpinAsync(int userId);
    Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default);
    Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default);
    Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreId, int actorId, int directorId);
    Task<Movie?> FindJackpotMovieAsync(int genreId, int actorId, int directorId);
    Task GrantJackpotDiscount(int userId, int movieId);
}