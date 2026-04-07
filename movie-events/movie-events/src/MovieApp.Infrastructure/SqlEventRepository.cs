namespace MovieApp.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// SQL Server–backed implementation of <see cref="IEventRepository"/>.
/// Handles data access operations for Events via ADO.NET.
/// </summary>
public sealed class SqlEventRepository : IEventRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlEventRepository"/> class.
    /// Initialises a new instance of the <see cref="SqlEventRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    public SqlEventRepository(DatabaseOptions databaseOptions)
    {
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously retrieves all events from the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of <see cref="Event"/> objects.</returns>
    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Event> events = new List<Event>();

        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(EventSqlQueries.SelectAll, connection);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            events.Add(MapEvent(sqlDataReader));
        }

        return events;
    }

    /// <summary>
    /// Asynchronously retrieves all events that match a specific event type.
    /// </summary>
    /// <param name="eventType">The type or category of the events to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of <see cref="Event"/> objects matching the provided type.</returns>
    public async Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        List<Event> events = new List<Event>();

        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(EventSqlQueries.SelectByType, connection);
        sqlCommand.Parameters.AddWithValue("@eventType", eventType);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            events.Add(MapEvent(sqlDataReader));
        }

        return events;
    }

    /// <summary>
    /// Asynchronously inserts a new event into the database.
    /// </summary>
    /// <param name="event">The <see cref="Event"/> to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The newly generated database identity ID for the inserted event.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the database fails to return the new identity value.</exception>
    public async Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(EventSqlQueries.Insert, connection);
        sqlCommand.Parameters.AddWithValue("@title", @event.Title);
        sqlCommand.Parameters.AddWithValue("@description", (object?)@event.Description ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@posterUrl", @event.PosterUrl);
        sqlCommand.Parameters.AddWithValue("@eventDateTime", @event.EventDateTime);
        sqlCommand.Parameters.AddWithValue("@locationReference", @event.LocationReference);
        sqlCommand.Parameters.AddWithValue("@ticketPrice", @event.TicketPrice);
        sqlCommand.Parameters.AddWithValue("@eventType", @event.EventType);
        sqlCommand.Parameters.AddWithValue("@historicalRating", @event.HistoricalRating);
        sqlCommand.Parameters.AddWithValue("@maxCapacity", @event.MaxCapacity);
        sqlCommand.Parameters.AddWithValue("@currentEnrollment", @event.CurrentEnrollment);
        sqlCommand.Parameters.AddWithValue("@creatorUserId", @event.CreatorUserId);

        object? result = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull)
        {
            throw new InvalidOperationException("Expected the event insert to return the new identity value.");
        }

        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Asynchronously finds an event by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The <see cref="Event"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(EventSqlQueries.SelectById, connection);
        sqlCommand.Parameters.AddWithValue("@id", id);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        if (!await sqlDataReader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapEvent(sqlDataReader);
    }

    /// <summary>
    /// Asynchronously updates all fields of an existing event, including the CreatorUserId.
    /// </summary>
    /// <param name="event">The <see cref="Event"/> containing the updated data.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the event was successfully updated; otherwise, <c>false</c>.</returns>
    public async Task<bool> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Events
            SET Title = @title,
                Description = @description,
                PosterUrl = @posterUrl,
                EventDateTime = @eventDateTime,
                LocationReference = @locationReference,
                TicketPrice = @ticketPrice,
                EventType = @eventType,
                HistoricalRating = @historicalRating,
                MaxCapacity = @maxCapacity,
                CurrentEnrollment = @currentEnrollment,
                CreatorUserId = @creatorUserId
            WHERE Id = @id;
            """;

        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sql, connection);
        sqlCommand.Parameters.AddWithValue("@id", @event.Id);
        sqlCommand.Parameters.AddWithValue("@title", @event.Title);
        sqlCommand.Parameters.AddWithValue("@description", (object?)@event.Description ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@posterUrl", @event.PosterUrl ?? string.Empty);
        sqlCommand.Parameters.AddWithValue("@eventDateTime", @event.EventDateTime);
        sqlCommand.Parameters.AddWithValue("@locationReference", @event.LocationReference ?? string.Empty);
        sqlCommand.Parameters.AddWithValue("@ticketPrice", @event.TicketPrice);
        sqlCommand.Parameters.AddWithValue("@eventType", @event.EventType ?? string.Empty);
        sqlCommand.Parameters.AddWithValue("@historicalRating", @event.HistoricalRating);
        sqlCommand.Parameters.AddWithValue("@maxCapacity", @event.MaxCapacity);
        sqlCommand.Parameters.AddWithValue("@currentEnrollment", @event.CurrentEnrollment);
        sqlCommand.Parameters.AddWithValue("@creatorUserId", @event.CreatorUserId);

        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    /// <summary>
    /// Asynchronously updates only the current enrollment count for a specific event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to update.</param>
    /// <param name="newCount">The new enrollment count.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the enrollment count was successfully updated; otherwise, <c>false</c>.</returns>
    public async Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Events
            SET CurrentEnrollment = @newCount
            WHERE Id = @eventId;
            """;

        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sql, connection);
        sqlCommand.Parameters.AddWithValue("@newCount", newCount);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    /// <summary>
    /// Updates all editable fields of an event except <c>CreatorUserId</c>.
    /// </summary>
    /// <param name="updatedEvent">The event containing the new field values.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <remarks>
    /// Prefer <see cref="UpdateAsync"/> when you also need to update <c>CreatorUserId</c>
    /// or require a return value indicating whether a row was matched.
    /// This overload exists for callers that do not need to update <c>CreatorUserId</c>.
    /// </remarks>
    /// /// <returns name="Task">Returns a Task.</returns>
    public async Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Events
            SET Title             = @title,
                Description       = @description,
                PosterUrl         = @posterUrl,
                EventDateTime     = @eventDateTime,
                LocationReference = @locationReference,
                TicketPrice       = @ticketPrice,
                EventType         = @eventType,
                HistoricalRating  = @historicalRating,
                MaxCapacity       = @maxCapacity,
                CurrentEnrollment = @currentEnrollment
            WHERE Id = @id;
            """;

        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sql, connection);
        sqlCommand.Parameters.AddWithValue("@id", updatedEvent.Id);
        sqlCommand.Parameters.AddWithValue("@title", updatedEvent.Title);
        sqlCommand.Parameters.AddWithValue("@description", updatedEvent.Description ?? (object)DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@posterUrl", updatedEvent.PosterUrl);
        sqlCommand.Parameters.AddWithValue("@eventDateTime", updatedEvent.EventDateTime);
        sqlCommand.Parameters.AddWithValue("@locationReference", updatedEvent.LocationReference);
        sqlCommand.Parameters.AddWithValue("@ticketPrice", updatedEvent.TicketPrice);
        sqlCommand.Parameters.AddWithValue("@eventType", updatedEvent.EventType);
        sqlCommand.Parameters.AddWithValue("@historicalRating", updatedEvent.HistoricalRating);
        sqlCommand.Parameters.AddWithValue("@maxCapacity", updatedEvent.MaxCapacity);
        sqlCommand.Parameters.AddWithValue("@currentEnrollment", updatedEvent.CurrentEnrollment);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously deletes an event and all of its related records safely using a database transaction.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the event was successfully deleted; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method ensures referential integrity by deleting foreign key dependencies
    /// (Screenings, Participations, FavoriteEvents, ReferralLogs) before removing the event.
    /// If any deletion fails, the transaction is rolled back.
    /// </remarks>
    /// /// <returns name="Task">Returns a Task.</returns>
    public async Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            await using SqlCommand deleteScreenings = new SqlCommand(
                "DELETE FROM Screenings WHERE EventId = @Id", connection, transaction);
            deleteScreenings.Parameters.AddWithValue("@Id", eventId);
            await deleteScreenings.ExecuteNonQueryAsync(cancellationToken);

            await using SqlCommand deleteParticipants = new SqlCommand(
                "DELETE FROM Participations WHERE EventId = @Id", connection, transaction);
            deleteParticipants.Parameters.AddWithValue("@Id", eventId);
            await deleteParticipants.ExecuteNonQueryAsync(cancellationToken);

            await using SqlCommand deleteFavorites = new SqlCommand(
                "DELETE FROM FavoriteEvents WHERE EventId = @Id", connection, transaction);
            deleteFavorites.Parameters.AddWithValue("@Id", eventId);
            await deleteFavorites.ExecuteNonQueryAsync(cancellationToken);

            await using SqlCommand deleteReferrals = new SqlCommand(
                "DELETE FROM ReferralLog WHERE EventId = @Id", connection, transaction);
            deleteReferrals.Parameters.AddWithValue("@Id", eventId);
            await deleteReferrals.ExecuteNonQueryAsync(cancellationToken);

            await using SqlCommand deleteEvent = new SqlCommand(
                "DELETE FROM Events WHERE Id = @Id", connection, transaction);
            deleteEvent.Parameters.AddWithValue("@Id", eventId);

            int rows = await deleteEvent.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return rows > 0;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Maps an event row using the column order defined in <see cref="EventSqlQueries.Projection"/>.
    /// </summary>
    /// <param name="sqlDataReader">The active data reader positioned on an event row.</param>
    /// <returns>A mapped <see cref="Event"/> object.</returns>
    private static Event MapEvent(SqlDataReader sqlDataReader)
    {
        return new Event
        {
            Id = sqlDataReader.GetInt32(0),
            Title = sqlDataReader.GetString(1),
            Description = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetString(2),
            PosterUrl = sqlDataReader.GetString(3),
            EventDateTime = sqlDataReader.GetDateTime(4),
            LocationReference = sqlDataReader.GetString(5),
            TicketPrice = sqlDataReader.GetDecimal(6),
            HistoricalRating = sqlDataReader.GetDouble(7),
            EventType = sqlDataReader.GetString(8),
            MaxCapacity = sqlDataReader.GetInt32(9),
            CurrentEnrollment = sqlDataReader.GetInt32(10),
            CreatorUserId = sqlDataReader.GetInt32(11),
        };
    }
}