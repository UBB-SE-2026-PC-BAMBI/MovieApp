// <copyright file="SqlMarathonRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;

using MovieApp.Core.Repositories;
using MovieApp.Core.Models;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed implementation of <see cref="IMarathonRepository"/>.
/// </summary>
public sealed class SqlMarathonRepository : IMarathonRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initialises a new instance of <see cref="SqlMarathonRepository"/>.
    /// </summary>
    /// <param name="databaseOptions">Database connection options.</param>
    public SqlMarathonRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);

        _connectionString = databaseOptions.ConnectionString;
    }


    /// <inheritdoc/>
    public async Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
    {
        List<Marathon> marathons = new List<Marathon>();
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "SELECT Id, Title, Description, PosterUrl, Theme, PrerequisiteMarathonId, WeekScoping " +
            "FROM dbo.Marathons WHERE IsActive = 1", connection);

        await connection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            marathons.Add(new Marathon
            {
                Id                     = sqlDataReader.GetInt32(0),
                Title                  = sqlDataReader.GetString(1),
                Description            = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetString(2),
                PosterUrl              = sqlDataReader.GetString(3),
                Theme                  = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetString(4),
                PrerequisiteMarathonId = sqlDataReader.IsDBNull(5) ? null : sqlDataReader.GetInt32(5),
                WeekScoping            = sqlDataReader.IsDBNull(6) ? null : sqlDataReader.GetString(6)
            });
        }
        return marathons;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
    {
        List<MarathonProgress> rankings = new List<MarathonProgress>();
        await using SqlConnection connection = new SqlConnection(_connectionString);
        const string sqlStringCommand = """
            SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt
            FROM dbo.MarathonProgress
            WHERE MarathonId = @MarathonId
            ORDER BY CompletedMoviesCount DESC,
                     TriviaAccuracy DESC,
                     FinishedAt ASC;
        """; 
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);

        sqlCommand.Parameters.AddWithValue("@MarathonId", marathonId);

        await connection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            rankings.Add(new MarathonProgress
            {
                UserId               = sqlDataReader.GetInt32(0),
                MarathonId           = sqlDataReader.GetInt32(1),
                TriviaAccuracy       = sqlDataReader.GetDouble(2),
                CompletedMoviesCount = sqlDataReader.GetInt32(3),
                FinishedAt           = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetDateTime(4)
            });
        }
        return rankings;
    }

    /// <inheritdoc/>
    public async Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "SELECT UserId, MarathonId, TriviaAccuracy, CompletedMoviesCount, FinishedAt " +
            "FROM dbo.MarathonProgress WHERE UserId = @UserId AND MarathonId = @MarathonId", connection);

        sqlCommand.Parameters.AddWithValue("@UserId", userId);
        sqlCommand.Parameters.AddWithValue("@MarathonId", marathonId);

        await connection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        if (await sqlDataReader.ReadAsync())
        {
            return new MarathonProgress
            {
                UserId               = sqlDataReader.GetInt32(0),
                MarathonId           = sqlDataReader.GetInt32(1),
                TriviaAccuracy       = sqlDataReader.GetDouble(2),
                CompletedMoviesCount = sqlDataReader.GetInt32(3),
                FinishedAt           = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetDateTime(4)
            };
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<bool> JoinMarathonAsync(int userId, int marathonId)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "INSERT INTO dbo.MarathonProgress (UserId, MarathonId, JoinedAt) " +
            "VALUES (@UserId, @MarathonId, @JoinedAt)", connection);

        sqlCommand.Parameters.AddWithValue("@UserId", userId);
        sqlCommand.Parameters.AddWithValue("@MarathonId", marathonId);
        sqlCommand.Parameters.AddWithValue("@JoinedAt", DateTime.Now);

        await connection.OpenAsync();
        return await sqlCommand.ExecuteNonQueryAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateProgressAsync(MarathonProgress progress)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(
            "UPDATE dbo.MarathonProgress SET " +
            "TriviaAccuracy = @Accuracy, CompletedMoviesCount = @Count, FinishedAt = @FinishedAt " +
            "WHERE UserId = @UserId AND MarathonId = @MarathonId", connection);

        sqlCommand.Parameters.AddWithValue("@Accuracy", progress.TriviaAccuracy);
        sqlCommand.Parameters.AddWithValue("@Count", progress.CompletedMoviesCount);
        sqlCommand.Parameters.AddWithValue("@FinishedAt", (object?)progress.FinishedAt ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@UserId", progress.UserId);
        sqlCommand.Parameters.AddWithValue("@MarathonId", progress.MarathonId);

        await connection.OpenAsync();
        return await sqlCommand.ExecuteNonQueryAsync() > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> IsPrerequisiteCompletedAsync(
    int userId, int prerequisiteMarathonId)
    {
        const string sqlStringCommand = """
            SELECT COUNT(1) FROM dbo.MarathonProgress
            WHERE UserId = @userId
              AND MarathonId = @prereqId
              AND FinishedAt IS NOT NULL;
        """;

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@prereqId", prerequisiteMarathonId);

        object? result = await sqlCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    /// <inheritdoc/>
    public async Task<int> GetMarathonMovieCountAsync(int marathonId)
    {
        const string sqlStringCommand = """
        SELECT COUNT(1) FROM dbo.MarathonMovies
        WHERE MarathonId = @marathonId;
        """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        object? result = await sqlCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static (string weekString, DateTime weekStart, DateTime weekEnd) GetCurrentWeekBounds()
    {
        DateTime now = DateTime.UtcNow;
        string? weekString = $"{now.Year}-W" +
            System.Globalization.ISOWeek.GetWeekOfYear(now).ToString("D2");

        int daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        DateTime monday = now.Date.AddDays(-daysFromMonday);
        DateTime sunday = monday.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

        return (weekString, monday, sunday);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(
    int userId, string weekString)
    {
        (string _,DateTime _,DateTime weekEnd) = GetCurrentWeekBounds();

        if (DateTime.UtcNow > weekEnd)
            return [];

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
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);
        sqlCommand.Parameters.AddWithValue("@week", weekString);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await connection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            marathons.Add(new Marathon
            {
                Id                     = sqlDataReader.GetInt32(0),
                Title                  = sqlDataReader.GetString(1),
                Description            = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetString(2),
                PosterUrl              = sqlDataReader.GetString(3),
                Theme                  = sqlDataReader.IsDBNull(4) ? null : sqlDataReader.GetString(4),
                PrerequisiteMarathonId = sqlDataReader.IsDBNull(5) ? null : sqlDataReader.GetInt32(5),
                WeekScoping            = sqlDataReader.IsDBNull(6) ? null : sqlDataReader.GetString(6),
            });
        }
        return marathons;
    }
    public async Task AssignWeeklyMarathonsAsync(
    int userId, string weekString, int count = 10)
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

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using SqlCommand selectCmd = new SqlCommand(selectSql, connection);
        selectCmd.Parameters.AddWithValue("@count", count);
        selectCmd.Parameters.AddWithValue("@userId", userId);
        selectCmd.Parameters.AddWithValue("@week", weekString);

        await using SqlDataReader sqlDataReader = await selectCmd.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
            ids.Add(sqlDataReader.GetInt32(0));

        await sqlDataReader.CloseAsync();

        if (ids.Count == 0) return;

        const string deactivateSql = """
            UPDATE dbo.Marathons
            SET IsActive = 0
            WHERE IsActive = 1
              AND WeekScoping <> @week;
        """;

        await using SqlCommand deactivateCmd = new SqlCommand(deactivateSql, connection);
        deactivateCmd.Parameters.AddWithValue("@week", weekString);
        await deactivateCmd.ExecuteNonQueryAsync();

        foreach (int id in ids)
        {
            const string updateSql = """
                UPDATE dbo.Marathons
                SET WeekScoping = @week, IsActive = 1
                WHERE Id = @id;
            """;

            await using SqlCommand updateCmd = new SqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@week", weekString);
            updateCmd.Parameters.AddWithValue("@id", id);
            await updateCmd.ExecuteNonQueryAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MovieApp.Core.Models.Movie.Movie>> GetMoviesForMarathonAsync(
    int marathonId)
    {
        const string sqlStringCommand = """
            SELECT m.Id, m.Title, m.Description, m.ReleaseYear, m.DurationMinutes
            FROM dbo.Movies m
            INNER JOIN dbo.MarathonMovies mm ON m.Id = mm.MovieId
            WHERE mm.MarathonId = @marathonId
            ORDER BY m.Title;
        """;

        List<MovieApp.Core.Models.Movie.Movie> movies = new List<MovieApp.Core.Models.Movie.Movie>();
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        await connection.OpenAsync();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync();
        while (await sqlDataReader.ReadAsync())
        {
            movies.Add(new MovieApp.Core.Models.Movie.Movie
            {
                Id              = sqlDataReader.GetInt32(0),
                Title           = sqlDataReader.GetString(1),
                Description     = sqlDataReader.IsDBNull(2) ? string.Empty : sqlDataReader.GetString(2),
                ReleaseYear     = sqlDataReader.IsDBNull(3) ? 0 : sqlDataReader.GetInt32(3),
                DurationMinutes = sqlDataReader.IsDBNull(4) ? 0 : sqlDataReader.GetInt32(4),
            });
        }
        return movies;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardWithUsernamesAsync(
        int marathonId)
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
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        await connection.OpenAsync();
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

    /// <inheritdoc/>
    public async Task<int> GetParticipantCountAsync(int marathonId)
    {
        const string sqlStringCommand = """
            SELECT COUNT(1) FROM dbo.MarathonProgress
            WHERE MarathonId = @marathonId;
        """;

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, connection);
        sqlCommand.Parameters.AddWithValue("@marathonId", marathonId);

        object? result = await sqlCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
