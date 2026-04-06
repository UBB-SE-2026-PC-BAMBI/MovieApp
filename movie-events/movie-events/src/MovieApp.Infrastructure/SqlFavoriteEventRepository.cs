using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// sqlStringCommand Server implementation of <see cref="IFavoriteEventRepository"/>.
/// </summary>
public sealed class SqlFavoriteEventRepository : IFavoriteEventRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Creates the repository using the configured database sqlConnection string.
    /// </summary>
    public SqlFavoriteEventRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <inheritdoc />
    public async Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            INSERT INTO dbo.FavoriteEvents (UserId, EventId)
            VALUES (@userId, @eventId);
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            DELETE FROM dbo.FavoriteEvents
            WHERE UserId = @userId
              AND EventId = @eventId;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT Id, UserId, EventId, CreatedAt
            FROM dbo.FavoriteEvents
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        return await ReadFavoriteEventsAsync(sqlCommand, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT TOP 1 1
            FROM dbo.FavoriteEvents
            WHERE UserId = @userId
              AND EventId = @eventId;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        var result = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT UserId
            FROM dbo.FavoriteEvents
            WHERE EventId = @eventId;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        
        List<int> userIds = new List<int>();

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            userIds.Add(sqlDataReader.GetInt32(0));
        }

        return userIds;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT Id, UserId, EventId, CreatedAt
            FROM dbo.FavoriteEvents
            WHERE EventId = @eventId
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        return await ReadFavoriteEventsAsync(sqlCommand, cancellationToken);
    }

    private static async Task<IReadOnlyList<FavoriteEvent>> ReadFavoriteEventsAsync(SqlCommand sqlCommand, CancellationToken cancellationToken)
    {
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        List<FavoriteEvent> favoriteEvents = new List<FavoriteEvent>();

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            favoriteEvents.Add(new FavoriteEvent
            {
                Id = sqlDataReader.GetInt32(0),
                UserId = sqlDataReader.GetInt32(1),
                EventId = sqlDataReader.GetInt32(2),
                CreatedAt = sqlDataReader.GetDateTime(3),
            });
        }

        return favoriteEvents;
    }
}
