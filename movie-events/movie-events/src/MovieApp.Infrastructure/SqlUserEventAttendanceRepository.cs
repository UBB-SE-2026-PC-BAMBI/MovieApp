namespace MovieApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using MovieApp.Core.Repositories;

/// <summary>
/// A SQL Server-backed repository for managing user event attendance via ADO.NET.
/// </summary>
public sealed class SqlUserEventAttendanceRepository : IUserEventAttendanceRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlUserEventAttendanceRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    /// <exception cref="ArgumentNullException">Thrown if the databaseOptions parameter is null.</exception>
    public SqlUserEventAttendanceRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously retrieves the IDs of all events a specific user has joined.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of event IDs.</returns>
    public async Task<IReadOnlyList<int>> GetJoinedEventIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "SELECT EventId FROM dbo.UserEventAttendance WHERE UserId = @userId";

        List<int> eventIds = new List<int>();

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            eventIds.Add(sqlDataReader.GetInt32(0));
        }

        return eventIds;
    }

    /// <summary>
    /// Asynchronously registers a user as attending a specific event.
    /// </summary>
    /// <param name="userId">The unique identifier of the user joining the event.</param>
    /// <param name="eventId">The unique identifier of the event to join.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <remarks>
    /// This method is idempotent. It will safely do nothing if the user is already registered for the specified event.
    /// </remarks>
    /// <returns>Returns a Task.</returns>
    public async Task JoinAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            IF NOT EXISTS (
                SELECT 1 FROM dbo.UserEventAttendance
                WHERE UserId = @userId AND EventId = @eventId
            )
            INSERT INTO dbo.UserEventAttendance (UserId, EventId)
            VALUES (@userId, @eventId)";

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}