using MovieApp.Core.Models.Movie;

namespace MovieApp.Core.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns movies that match ANY of the given genre, actor, or director (OR logic).
    /// </summary>
    Task<IReadOnlyList<Movie>> FindMoviesByAnyCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all valid (Genre, Actor, Director) combinations from movies
    /// that have at least one screening in a future event.
    /// </summary>
    Task<IReadOnlyList<ReelCombination>> GetValidReelCombinationsAsync(CancellationToken cancellationToken = default);
}
