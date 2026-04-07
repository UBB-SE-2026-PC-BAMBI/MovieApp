using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Tests.Fakes;

public sealed class FakeMarathonRepository : IMarathonRepository
{
    public List<Marathon> ActiveMarathons { get; set; } = [];

    public Dictionary<int, MarathonProgress> ProgressByMarathonId { get; } = [];

    public Dictionary<int, int> MovieCountsByMarathonId { get; } = [];

    public bool AssignWeeklyMarathonsCalled { get; private set; }

    public bool JoinMarathonCalled { get; private set; }

    public bool IsPrerequisiteCompletedResult { get; set; } = true;

    public MarathonProgress? UpdatedProgress { get; private set; }

    public Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
    {
        return Task.FromResult<IEnumerable<Marathon>>(ActiveMarathons);
    }

    public Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
    {
        ProgressByMarathonId.TryGetValue(marathonId, out var progress);
        return Task.FromResult(progress);
    }

    public Task<bool> JoinMarathonAsync(int userId, int marathonId)
    {
        JoinMarathonCalled = true;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateProgressAsync(MarathonProgress progress)
    {
        UpdatedProgress = progress;
        ProgressByMarathonId[progress.MarathonId] = progress;
        return Task.FromResult(true);
    }

    public Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
    {
        return Task.FromResult<IEnumerable<MarathonProgress>>([]);
    }

    public Task<bool> IsPrerequisiteCompletedAsync(int userId, int prerequisiteMarathonId)
    {
        return Task.FromResult(IsPrerequisiteCompletedResult);
    }

    public Task<int> GetMarathonMovieCountAsync(int marathonId)
    {
        return Task.FromResult(MovieCountsByMarathonId.TryGetValue(marathonId, out int count) ? count : 0);
    }

    public Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(int userId, string weekString)
    {
        IEnumerable<Marathon> result = AssignWeeklyMarathonsCalled
            ? [new Marathon { Id = 1, Title = "Weekly Pick", IsActive = true, WeekScoping = weekString }]
            : [];

        return Task.FromResult(result);
    }

    public Task AssignWeeklyMarathonsAsync(int userId, string weekString, int count = 10)
    {
        AssignWeeklyMarathonsCalled = true;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Movie>> GetMoviesForMarathonAsync(int marathonId)
    {
        return Task.FromResult<IEnumerable<Movie>>([]);
    }

    public Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId)
    {
        return Task.FromResult<IEnumerable<LeaderboardEntry>>([]);
    }

    public Task<int> GetParticipantCountAsync(int marathonId)
    {
        return Task.FromResult(0);
    }
}