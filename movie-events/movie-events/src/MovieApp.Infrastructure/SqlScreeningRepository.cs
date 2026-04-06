using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// A SQL Server-backed repository for managing screenings that map movies to events.
/// Handles database operations via ADO.NET.
/// </summary>
public sealed class SqlScreeningRepository : IScreeningRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlScreeningRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    /// <exception cref="ArgumentNullException">Thrown if the databaseOptions parameter is null.</exception>
    public SqlScreeningRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously retrieves all movie screenings scheduled for a specific event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="Screening"/> objects associated with the event.</returns>
    public async Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT Id, EventId, MovieId, ScreeningTime
            FROM dbo.Screenings
            WHERE EventId = @eventId";

        List<Screening> screenings = new List<Screening>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            screenings.Add(new Screening
            {
                Id = sqlDataReader.GetInt32(0),
                EventId = sqlDataReader.GetInt32(1),
                MovieId = sqlDataReader.GetInt32(2),
                ScreeningTime = sqlDataReader.GetDateTime(3)
            });
        }

        return screenings;
    }

    /// <summary>
    /// Asynchronously retrieves all scheduled screenings for a specific movie across all events.
    /// </summary>
    /// <param name="movieId">The unique identifier of the movie.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="Screening"/> objects associated with the movie.</returns>
    public async Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT Id, EventId, MovieId, ScreeningTime
            FROM dbo.Screenings
            WHERE MovieId = @movieId";

        List<Screening> screenings = new List<Screening>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@movieId", movieId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            screenings.Add(new Screening
            {
                Id = sqlDataReader.GetInt32(0),
                EventId = sqlDataReader.GetInt32(1),
                MovieId = sqlDataReader.GetInt32(2),
                ScreeningTime = sqlDataReader.GetDateTime(3)
            });
        }

        return screenings;
    }

    /// <summary>
    /// Asynchronously inserts a new screening record into the database, linking a movie to an event.
    /// </summary>
    /// <param name="screening">The <see cref="Screening"/> object containing the event ID, movie ID, and screening time.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task AddAsync(Screening screening, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            INSERT INTO dbo.Screenings (EventId, MovieId, ScreeningTime)
            VALUES (@eventId, @movieId, @screeningTime)";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@eventId", screening.EventId);
        sqlCommand.Parameters.AddWithValue("@movieId", screening.MovieId);
        sqlCommand.Parameters.AddWithValue("@screeningTime", screening.ScreeningTime);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}