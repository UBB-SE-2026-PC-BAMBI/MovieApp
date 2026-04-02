using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server-backed repository for managing screenings that map movies to events.
/// </summary>
public sealed class SqlScreeningRepository : IScreeningRepository
{
    private readonly string _connectionString;

    public SqlScreeningRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IReadOnlyList<Screening>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EventId, MovieId, ScreeningTime
            FROM dbo.Screenings
            WHERE EventId = @eventId";

        var screenings = new List<Screening>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eventId", eventId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            screenings.Add(new Screening
            {
                Id = reader.GetInt32(0),
                EventId = reader.GetInt32(1),
                MovieId = reader.GetInt32(2),
                ScreeningTime = reader.GetDateTime(3)
            });
        }

        return screenings;
    }

    public async Task<IReadOnlyList<Screening>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EventId, MovieId, ScreeningTime
            FROM dbo.Screenings
            WHERE MovieId = @movieId";

        var screenings = new List<Screening>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@movieId", movieId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            screenings.Add(new Screening
            {
                Id = reader.GetInt32(0),
                EventId = reader.GetInt32(1),
                MovieId = reader.GetInt32(2),
                ScreeningTime = reader.GetDateTime(3)
            });
        }

        return screenings;
    }

    public async Task AddAsync(Screening screening, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.Screenings (EventId, MovieId, ScreeningTime)
            VALUES (@eventId, @movieId, @screeningTime)";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@eventId", screening.EventId);
        command.Parameters.AddWithValue("@movieId", screening.MovieId);
        command.Parameters.AddWithValue("@screeningTime", screening.ScreeningTime);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
