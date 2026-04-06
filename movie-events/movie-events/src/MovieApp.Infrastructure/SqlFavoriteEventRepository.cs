using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server implementation of <see cref="IFavoriteEventRepository"/>.
/// Provides methods for managing user favorite events in the database.
/// </summary>
public sealed class SqlFavoriteEventRepository : IFavoriteEventRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlFavoriteEventRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database configuration options containing the connection string.</param>
    public SqlFavoriteEventRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Adds a favorite event entry for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Inserts a new record into the <c>FavoriteEvents</c> table.
    /// </remarks>
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

    /// <summary>
    /// Removes a favorite event entry for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Deletes the matching record from the <c>FavoriteEvents</c> table if it exists.
    /// </remarks>
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

    /// <summary>
    /// Retrieves all favorite events for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="FavoriteEvent"/> objects associated with the user,
    /// ordered by creation date descending.
    /// </returns>
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

    /// <summary>
    /// Determines whether a favorite event entry exists for a given user and event.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// <c>true</c> if a matching favorite event exists; otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Retrieves all user IDs that have marked a specific event as favorite.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of user IDs who have favorited the specified event.
    /// </returns>
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

    /// <summary>
    /// Retrieves all favorite event entries for a given event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="FavoriteEvent"/> objects associated with the event,
    /// ordered by creation date descending.
    /// </returns>
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

    /// <summary>
    /// Executes the provided SQL command and maps the result set to a list of <see cref="FavoriteEvent"/> objects.
    /// </summary>
    /// <param name="sqlCommand">The SQL command to execute.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of mapped <see cref="FavoriteEvent"/> instances.
    /// </returns>
    /// <remarks>
    /// This method assumes the query returns columns in the following order:
    /// Id, UserId, EventId, CreatedAt.
    /// </remarks>
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