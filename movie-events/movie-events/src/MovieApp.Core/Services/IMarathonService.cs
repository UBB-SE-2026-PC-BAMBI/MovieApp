using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;

namespace MovieApp.Core.Services;

public interface IMarathonService
{
    Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userId);

    Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId);

    Task<bool> StartMarathonAsync(int marathonId);

    Task UpdateQuizResultAsync(int marathonId, int correctAnswers);

    Task<bool> LogMovieAsync(int marathonId, int movieId, int correctAnswers);

    Task<int> GetParticipantCountAsync(int marathonId);

    Task<int> GetMarathonMovieCountAsync(int marathonId);

    Task<bool> IsPrerequisiteCompletedAsync(int userId, int marathonId);

    Task<IEnumerable<Movie>> GetMoviesForMarathonAsync(int marathonId);

    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync(int marathonId);

    Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId);

    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId);
}