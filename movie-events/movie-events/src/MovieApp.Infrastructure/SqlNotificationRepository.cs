using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlNotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public SqlNotificationRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            INSERT INTO dbo.Notifications (UserId, EventId, Type, Message, State, CreatedAt)
            VALUES (@userId, @eventId, @type, @message, @state, @createdAt);
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", notification.UserId);
        sqlCommand.Parameters.AddWithValue("@eventId", notification.EventId);
        sqlCommand.Parameters.AddWithValue("@type", notification.Type);
        sqlCommand.Parameters.AddWithValue("@message", notification.Message);
        sqlCommand.Parameters.AddWithValue("@state", notification.State.ToString());
        sqlCommand.Parameters.AddWithValue("@createdAt", notification.CreatedAt);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            DELETE FROM dbo.Notifications
            WHERE Id = @id;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", notificationId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT Id, UserId, EventId, Type, Message, State, CreatedAt
            FROM dbo.Notifications
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        List<Notification> notifications = new List<Notification>();

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            string stateText = sqlDataReader.GetString(5);
            _ = Enum.TryParse<NotificationState>(stateText, ignoreCase: true, out var state);

            notifications.Add(new Notification
            {
                Id        = sqlDataReader.GetInt32(0),
                UserId    = sqlDataReader.GetInt32(1),
                EventId   = sqlDataReader.GetInt32(2),
                Type      = sqlDataReader.GetString(3),
                Message   = sqlDataReader.GetString(4),
                State     = state,
                CreatedAt = sqlDataReader.GetDateTime(6),
            });
        }

        return notifications;
    }
}
