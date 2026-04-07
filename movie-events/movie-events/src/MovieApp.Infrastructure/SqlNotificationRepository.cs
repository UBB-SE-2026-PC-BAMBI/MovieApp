// <copyright file="SqlNotificationRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// A SQL Server-backed repository for managing user notifications via ADO.NET.
/// </summary>
public sealed class SqlNotificationRepository : INotificationRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlNotificationRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    public SqlNotificationRepository(DatabaseOptions databaseOptions)
    {
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously inserts a new notification into the database.
    /// </summary>
    /// <param name="notification">The <see cref="Notification"/> to add.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <remarks>The notification's state is stored as its string representation.</remarks>
    /// <returns>Returns a Task.</returns>
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            INSERT INTO dbo.Notifications (UserId, EventId, Type, Message, State, CreatedAt)
            VALUES (@userId, @eventId, @type, @message, @state, @createdAt);
            """;

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
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

    /// <summary>
    /// Asynchronously deletes a specific notification from the database.
    /// </summary>
    /// <param name="notificationId">The unique identifier of the notification to remove.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>Returns a Task.</returns>
    public async Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            DELETE FROM dbo.Notifications
            WHERE Id = @id;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", notificationId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves all notifications belonging to a specific user, ordered from newest to oldest.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve notifications for.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="Notification"/> objects.</returns>
    public async Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT Id, UserId, EventId, Type, Message, State, CreatedAt
            FROM dbo.Notifications
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
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
                Id = sqlDataReader.GetInt32(0),
                UserId = sqlDataReader.GetInt32(1),
                EventId = sqlDataReader.GetInt32(2),
                Type = sqlDataReader.GetString(3),
                Message = sqlDataReader.GetString(4),
                State = state,
                CreatedAt = sqlDataReader.GetDateTime(6),
            });
        }

        return notifications;
    }
}