using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories
{
    public interface ITriviaRepository
    {
        Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieId, int count = 3, CancellationToken cancellationToken = default);
    }
}
