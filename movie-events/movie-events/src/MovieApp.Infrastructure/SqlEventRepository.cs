using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// SQL Server–backed implementation of <see cref="IEventRepository"/>.
/// </summary>
public sealed class SqlEventRepository : IEventRepository
{
    private readonly string _connectionString;


    /// <summary>
    /// Initialises a new instance of <see cref="SqlEventRepository"/>.
    /// </summary>
    /// <param name="databaseOptions">Database connection options.</param>
    public SqlEventRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Event> events = new List<Event>();

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(EventSqlQueries.SelectAll, connection);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            events.Add(MapEvent(sqlDataReader));
        }

        return events;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        List<Event> events = new List<Event>();

        await using SqlConnection connection = new SqlConnection(_connectionString);
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

    /// <inheritdoc/>
    public async Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
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

    /// <inheritdoc/>
    public async Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
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

    /// <inheritdoc/>
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

        await using SqlConnection connection = new SqlConnection(_connectionString);
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

    /// <inheritdoc/>
    public async Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Events
            SET CurrentEnrollment = @newCount
            WHERE Id = @eventId;
            """;

        await using SqlConnection connection = new SqlConnection(_connectionString);
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Prefer <see cref="UpdateAsync"/> when you also need to update <c>CreatorUserId</c>
    /// or require a return value indicating whether a row was matched.
    /// This overload exists for callers that do not need to update <c>CreatorUserId</c>.
    /// </remarks>
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

        await using SqlConnection connection = new SqlConnection(_connectionString);
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
    /// Maps an event row using the column order defined in <see cref="EventSqlQueries.Projection"/>.
    /// </summary>
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
            CreatorUserId = sqlDataReader.GetInt32(11)
        };
    }


    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand("DELETE FROM Events WHERE Id = @Id", connection);
        sqlCommand.Parameters.AddWithValue("@Id", eventId);

        int rows = await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }
}
