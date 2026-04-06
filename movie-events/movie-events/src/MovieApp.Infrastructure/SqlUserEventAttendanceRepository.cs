using Microsoft.Data.SqlClient;

using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlUserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly string _connectionString;

    public SqlUserEventAttendanceRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "SELECT EventId FROM dbo.UserEventAttendance WHERE UserId = @userId";

        List<int> eventIds = new List<int>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader SqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await SqlDataReader.ReadAsync(cancellationToken))
            eventIds.Add(SqlDataReader.GetInt32(0));

        return eventIds;
    }

    public async Task JoinAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            IF NOT EXISTS (
                SELECT 1 FROM dbo.UserEventAttendance
                WHERE UserId = @userId AND EventId = @eventId
            )
            INSERT INTO dbo.UserEventAttendance (UserId, EventId)
            VALUES (@userId, @eventId)";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}
