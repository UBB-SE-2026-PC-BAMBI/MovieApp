namespace MovieApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// SQL Server-backed implementation of <see cref="IMarathonRepository"/>.
/// Handles data access operations for marathons, user progress, leaderboards, and weekly assignments.
/// </summary>
public sealed class SqlMarathonRepository : IMarathonRepository
{
    private const int DaysInWeek = 7;
    private const int Days = 6;
    private const int Hours = 23;
    private const int Minutes = 59;
    private const int Seconds = 59;

    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMarathonRepository"/> class.
    /// Initialises a new instance of the <see cref="SqlMarathonRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">Database connection options containing the connection string.</param>
    /// <exception cref="ArgumentNullException">Thrown when databaseOptions is null.</exception>
    public SqlMarathonRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously retrieves all active marathons from the database.
    /// </summary>
    /// <returns>A collection of active <see cref="Marathon"/> objects.</returns>
    public async Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
    {
        List<Marathon> marathons = new List<Marathon>();
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "SELECT Id, Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, WeekScoping " +
            "FROM dbo.Marathons WHERE IsActive = 1", sqlConnection);

        await sqlConnection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            marathons.Add(new Marathon
            {
                Id = sqlDataReader.GetInt32(0),
                Title = sqlDataReader.GetString(1),
                Description = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetString(2),
                PosterUrl = sqlDataReader.GetString(3),
                Theme = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetString(4),
                PrerequisiteMarathonId = sqlDataReader.IsDBNull(5) ? null : sqlDataReader.GetInt32(5),
                WeekScoping = sqlDataReader.IsDBNull(6) ? null : sqlDataReader.GetString(6),
            });
        }

        return marathons;
    }

    /// <summary>
    /// Asynchronously retrieves the raw progress data to construct a leaderboard for a specific marathon.
    /// </summary>
    /// <param name="marathonId">The unique identifier of the marathon.</param>
    /// <returns>
    /// A collection of <see cref="MarathonProgress"/> objects ranked by completed movies (desc),
    /// trivia accuracy (desc), and completion time (asc).
    /// </returns>
    public async Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
    {
        List<MarathonProgress> rankings = new List<MarathonProgress>();
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        const string sqlStringCommand = """
            SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt
            FROM dbo.MarathonProgress
            WHERE MarathonId = @MarathonId
            ORDER BY CompletedMoviesCount DESC,
                     TriviaAccuracy DESC,
                     FinishedAt ASC;
        """;
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);

        sqlCommand.Parameters.AddWithValue("@MarathonId", marathonId);

        await sqlConnection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            rankings.Add(new MarathonProgress
            {
                UserId = sqlDataReader.GetInt32(0),
                MarathonId = sqlDataReader.GetInt32(1),
                TriviaAccuracy = sqlDataReader.GetDouble(2),
                CompletedMoviesCount = sqlDataReader.GetInt32(3),
                FinishedAt = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetDateTime(4),
            });
        }

        return rankings;
    }

    /// <summary>
    /// Asynchronously retrieves the specific marathon progress for a given user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="marathonId">The ID of the marathon.</param>
    /// <returns>The <see cref="MarathonProgress"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
    {
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt " +
            "FROM dbo.MarathonProgress WHERE UserId = @UserId AND MarathonId = @MarathonId", sqlConnection);

        sqlCommand.Parameters.AddWithValue("@UserId", userId);
        sqlCommand.Parameters.AddWithValue("@MarathonId", marathonId);

        await sqlConnection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        if (await sqlDataReader.ReadAsync())
        {
            return new MarathonProgress
            {
                UserId = sqlDataReader.GetInt32(0),
                MarathonId = sqlDataReader.GetInt32(1),
                TriviaAccuracy = sqlDataReader.GetDouble(2),
                CompletedMoviesCount = sqlDataReader.GetInt32(3),
                FinishedAt = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetDateTime(4),
            };
        }

        return null;
    }

    /// <summary>
    /// Asynchronously enrolls a user into a marathon by creating a progress record.
    /// </summary>
    /// <param name="userId">The ID of the joining user.</param>
    /// <param name="marathonId">The ID of the marathon to join.</param>
    /// <returns><c>true</c> if the user successfully joined; otherwise, <c>false</c>.</returns>
    public async Task<bool> JoinMarathonAsync(int userId, int marathonId)
    {
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "INSERT INTO dbo.MarathonProgress (UserId, MarathonId, JoinedAt) " +
            "VALUES (@UserId, @MarathonId, @JoinedAt)", sqlConnection);

        sqlCommand.Parameters.AddWithValue("@UserId", userId);
        sqlCommand.Parameters.AddWithValue("@MarathonId", marathonId);
        sqlCommand.Parameters.AddWithValue("@JoinedAt", DateTime.Now);

        await sqlConnection.OpenAsync();
        return await sqlCommand.ExecuteNonQueryAsync() > 0;
    }

    /// <summary>
    /// Asynchronously updates a user's ongoing progress within a marathon.
    /// </summary>
    /// <param name="progress">The updated <see cref="MarathonProgress"/> object.</param>
    /// <returns><c>true</c> if the progress was updated successfully; otherwise, <c>false</c>.</returns>
    public async Task<bool> UpdateProgressAsync(MarathonProgress progress)
    {
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "UPDATE dbo.MarathonProgress SET " +
            "TriviaAccuracy = @Accuracy, CompletedMoviesCount = @Count, FinishedAt = @FinishedAt " +
            "WHERE UserId = @UserId AND MarathonId = @MarathonId", sqlConnection);

        sqlCommand.Parameters.AddWithValue("@Accuracy", progress.TriviaAccuracy);
        sqlCommand.Parameters.AddWithValue("@Count", progress.CompletedMoviesCount);
        sqlCommand.Parameters.AddWithValue("@FinishedAt", (object?)progress.FinishedAt ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@UserId", progress.UserId);
        sqlCommand.Parameters.AddWithValue("@MarathonId", progress.MarathonId);

        await sqlConnection.OpenAsync();
        return await sqlCommand.ExecuteNonQueryAsync() > 0;
    }

    /// <summary>
    /// Asynchronously determines if a user has completed a required prerequisite marathon.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="prerequisiteMarathonId">The ID of the prerequisite marathon.</param>
    /// <returns><c>true</c> if the prerequisite is completed (FinishedAt is not null); otherwise, <c>false</c>.</returns>
    public async Task<bool> IsPrerequisiteCompletedAsync(int userId, int prerequisiteMarathonId)
    {
        const string sqlStringCommand = """
            SELECT COUNT(1) FROM dbo.MarathonProgress
            WHERE UserId = @userId
              AND MarathonId = @prereqId
              AND FinishedAt IS NOT NULL;
        """;

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@prereqId", prerequisiteMarathonId);

        object? result = await sqlCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    /// <summary>
    /// Asynchronously counts the total number of movies associated with a specific marathon.
    /// </summary>
    /// <param name="marathonId">The ID of the marathon.</param>
    /// <returns>The total movie count for the marathon.</returns>
    public async Task<int> GetMarathonMovieCountAsync(int marathonId)
    {
        const string sqlStringCommand = """
        SELECT COUNT(1) FROM dbo.MarathonMovies
        WHERE MarathonId = @marathonId;
        """;

        await using var sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        object? result = await sqlCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Asynchronously retrieves active weekly scoped marathons that a user has not yet finished.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="weekString">The ISO week string to query (e.g., "2024-W15").</param>
    /// <returns>A collection of available <see cref="Marathon"/> objects for the specified week.</returns>
    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(int userId, string weekString)
    {
        (string _, DateTime _, DateTime weekEnd) = GetCurrentWeekBounds();

        if (DateTime.UtcNow > weekEnd)
        {
            return [];
        }

        const string sqlStringCommand = """
            SELECT Id, Title, Description, PosterUrl, Theme,
                   PrerequisiteMarathonId, WeekScoping
            FROM dbo.Marathons
            WHERE IsActive = 1
              AND WeekScoping = @week
              AND Id NOT IN (
                  SELECT MarathonId FROM dbo.MarathonProgress
                  WHERE UserId = @userId
                    AND FinishedAt IS NOT NULL
              );
        """;

        List<Marathon> marathons = new List<Marathon>();
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@week", weekString);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await sqlConnection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            marathons.Add(new Marathon
            {
                Id = sqlDataReader.GetInt32(0),
                Title = sqlDataReader.GetString(1),
                Description = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetString(2),
                PosterUrl = sqlDataReader.GetString(3),
                Theme = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetString(4),
                PrerequisiteMarathonId = sqlDataReader.IsDBNull(5) ? null : sqlDataReader.GetInt32(5),
                WeekScoping = sqlDataReader.IsDBNull(6) ? null : sqlDataReader.GetString(6),
            });
        }

        return marathons;
    }

    /// <summary>
    /// Asynchronously assigns a random selection of unfinished marathons to be active for the specified week.
    /// Deactivates marathons from previous weeks.
    /// </summary>
    /// <param name="userId">The ID of the user used to exclude already finished marathons.</param>
    /// <param name="weekString">The target ISO week string.</param>
    /// <param name="count">The number of marathons to assign (default is 10).</param>
    /// <returns>Returns a Task.</returns>
    public async Task AssignWeeklyMarathonsAsync(int userId, string weekString, int count = 10)
    {
        const string selectSql = """
            SELECT TOP (@count) Id
            FROM dbo.Marathons
            WHERE Id NOT IN (
                SELECT MarathonId FROM dbo.MarathonProgress
                WHERE UserId = @userId AND FinishedAt IS NOT NULL
            )
            AND (WeekScoping IS NULL OR WeekScoping <> @week)
            AND PrerequisiteMarathonId IS NULL
            ORDER BY NEWID();
        """;

        List<int> ids = new List<int>();

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync();

        await using SqlCommand selectCmd = new SqlCommand(selectSql, sqlConnection);
        selectCmd.Parameters.AddWithValue("@count", count);
        selectCmd.Parameters.AddWithValue("@userId", userId);
        selectCmd.Parameters.AddWithValue("@week", weekString);

        await using SqlDataReader sqlDataReader = await selectCmd.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            ids.Add(sqlDataReader.GetInt32(0));
        }

        await sqlDataReader.CloseAsync();

        if (ids.Count == 0)
        { 
            return;
        }

        const string deactivateSql = """
            UPDATE dbo.Marathons
            SET IsActive = 0
            WHERE IsActive = 1
              AND WeekScoping <> @week;
        """;

        await using SqlCommand deactivateCommand = new SqlCommand(deactivateSql, sqlConnection);
        deactivateCommand.Parameters.AddWithValue("@week", weekString);
        await deactivateCommand.ExecuteNonQueryAsync();

        foreach (int id in ids)
        {
            const string updateSql = """
                UPDATE dbo.Marathons
                SET WeekScoping = @week, IsActive = 1
                WHERE Id = @id;
            """;

            await using SqlCommand updateCommand = new SqlCommand(updateSql, sqlConnection);
            updateCommand.Parameters.AddWithValue("@week", weekString);
            updateCommand.Parameters.AddWithValue("@id", id);
            await updateCommand.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Asynchronously retrieves all movies assigned to a specific marathon.
    /// </summary>
    /// <param name="marathonId">The ID of the marathon.</param>
    /// <returns>A collection of <see cref="MovieApp.Core.Models.Movie.Movie"/> objects ordered by title.</returns>
    public async Task<IEnumerable<MovieApp.Core.Models.Movie.Movie>> GetMoviesForMarathonAsync(int marathonId)
    {
        const string sqlStringCommand = """
            SELECT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
            FROM dbo.Movies m
            INNER JOIN dbo.MarathonMovies mm ON m.Id = mm.MovieId
            WHERE mm.MarathonId = @marathonId
            ORDER BY m.Title;
        """;

        List<MovieApp.Core.Models.Movie.Movie> movies = new List<MovieApp.Core.Models.Movie.Movie>();
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        await sqlConnection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            movies.Add(new MovieApp.Core.Models.Movie.Movie
            {
                Id = sqlDataReader.GetInt32(0),
                Title = sqlDataReader.GetString(1),
                Description = sqlDataReader.IsDBNull(2) ? string.Empty : sqlDataReader.GetString(2),
                ReleaseYear = sqlDataReader.IsDBNull(3) ? 0 : sqlDataReader.GetInt32(3),
                DurationMinutes = sqlDataReader.IsDBNull(4) ? 0 : sqlDataReader.GetInt32(4),
            });
        }

        return movies;
    }

    /// <summary>
    /// Asynchronously retrieves the marathon leaderboard along with the corresponding usernames.
    /// </summary>
    /// <param name="marathonId">The ID of the marathon.</param>
    /// <returns>A collection of <see cref="LeaderboardEntry"/> objects sorted by completion metrics.</returns>
    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(int marathonId)
    {
        const string sqlStringCommand = """
        SELECT u.Id, u.Username, mp.CompletedMoviesCount, mp.TriviaAccuracy, mp.FinishedAt
        FROM dbo.MarathonProgress mp
        INNER JOIN dbo.Users u ON mp.UserId = u.Id
        WHERE mp.MarathonId = @marathonId
        ORDER BY mp.CompletedMoviesCount DESC,
                 mp.TriviaAccuracy DESC,
                 mp.FinishedAt ASC;
        """;

        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        await sqlConnection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            entries.Add(new LeaderboardEntry
            {
                UserId = sqlDataReader.GetInt32(0),
                Username = sqlDataReader.GetString(1),
                CompletedMoviesCount = sqlDataReader.GetInt32(2),
                TriviaAccuracy = sqlDataReader.GetDouble(3),
                FinishedAt = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetDateTime(4),
            });
        }

        return entries;
    }

    /// <summary>
    /// Asynchronously counts the total number of users participating in a specific marathon.
    /// </summary>
    /// <param name="marathonId">The ID of the marathon.</param>
    /// <returns>The total count of participants.</returns>
    public async Task<int> GetParticipantCountAsync(int marathonId)
    {
        const string sqlStringCommand = """
            SELECT COUNT(1) FROM dbo.MarathonProgress
            WHERE MarathonId = @marathonId;
        """;

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        object? result = await sqlCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Calculates the ISO week string and start/end boundaries for the current UTC week.
    /// </summary>
    /// <returns>A tuple containing the ISO week string, Monday start date, and Sunday end date.</returns>
    private static (string weekString, DateTime weekStart, DateTime weekEnd) GetCurrentWeekBounds()
    {
        DateTime now = DateTime.UtcNow;
        string? weekString = $"{now.Year}-W" +
            System.Globalization.ISOWeek.GetWeekOfYear(now).ToString("D2");

        int daysFromMonday = ((int)now.DayOfWeek + Days) % DaysInWeek;
        DateTime monday = now.Date.AddDays(-daysFromMonday);
        DateTime sunday = monday.AddDays(Days).AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds);

        return (weekString, monday, sunday);
    }
}