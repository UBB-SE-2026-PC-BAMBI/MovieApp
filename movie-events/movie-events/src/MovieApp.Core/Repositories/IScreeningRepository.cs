using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IScreeningRepository
{
    Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken = default);

    Task AddAsync(Screening screening, CancellationToken cancellationToken = default);
}
