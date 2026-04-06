using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// sqlStringCommand Server-backed repository for managing screenings that map movies to events.
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
                Id            = sqlDataReader.GetInt32(0),
                EventId       = sqlDataReader.GetInt32(1),
                MovieId       = sqlDataReader.GetInt32(2),
                ScreeningTime = sqlDataReader.GetDateTime(3)
            });
        }

        return screenings;
    }

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
