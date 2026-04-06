using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// A SQL Server-backed repository for managing user slot-machine state. 
/// Handles data access to the dbo.UserSpins table and maps records to <see cref="UserSpinData"/>.
/// </summary>
public sealed class SqlUserSlotMachineStateRepository : IUserSlotMachineStateRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlUserSlotMachineStateRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    /// <exception cref="ArgumentNullException">Thrown if the databaseOptions parameter is null.</exception>
    public SqlUserSlotMachineStateRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously retrieves the slot machine spin data for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="UserSpinData"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "SELECT UserId, DailySpinsRemaining, BonusSpins, LastSlotSpinReset, LastTriviaSpinReset, LoginStreak, LastLoginDate, EventSpinRewardsToday FROM dbo.UserSpins WHERE UserId = @userId";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        if (!await sqlDataReader.ReadAsync(cancellationToken))
            return null;

        return new UserSpinData
        {
            UserId = sqlDataReader.GetInt32(0),
            DailySpinsRemaining = sqlDataReader.GetInt32(1),
            BonusSpins = sqlDataReader.GetInt32(2),
            LastSlotSpinReset = sqlDataReader.IsDBNull(3) ? default : sqlDataReader.GetDateTime(3),
            LastTriviaSpinReset = sqlDataReader.IsDBNull(4) ? default : sqlDataReader.GetDateTime(4),
            LoginStreak = sqlDataReader.GetInt32(5),
            LastLoginDate = sqlDataReader.IsDBNull(6) ? default : sqlDataReader.GetDateTime(6),
            EventSpinRewardsToday = sqlDataReader.GetInt32(7)
        };
    }

    /// <summary>
    /// Asynchronously creates a new slot machine state record for a user in the database.
    /// </summary>
    /// <param name="state">The <see cref="UserSpinData"/> object to insert.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <remarks>Uninitialized (default) DateTime fields are stored as database NULLs.</remarks>
    public async Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "INSERT INTO dbo.UserSpins (UserId, DailySpinsRemaining, BonusSpins, LastSlotSpinReset, LastTriviaSpinReset, LoginStreak, LastLoginDate, EventSpinRewardsToday) VALUES (@userId, @dailySpins, @bonusSpins, @lastSlotReset, @lastTriviaReset, @loginStreak, @lastLoginDate, @eventRewards)";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", state.UserId);
        sqlCommand.Parameters.AddWithValue("@dailySpins", state.DailySpinsRemaining);
        sqlCommand.Parameters.AddWithValue("@bonusSpins", state.BonusSpins);
        sqlCommand.Parameters.AddWithValue("@lastSlotReset", state.LastSlotSpinReset == default ? DBNull.Value : (object)state.LastSlotSpinReset);
        sqlCommand.Parameters.AddWithValue("@lastTriviaReset", state.LastTriviaSpinReset == default ? DBNull.Value : (object)state.LastTriviaSpinReset);
        sqlCommand.Parameters.AddWithValue("@loginStreak", state.LoginStreak);
        sqlCommand.Parameters.AddWithValue("@lastLoginDate", state.LastLoginDate == default ? DBNull.Value : (object)state.LastLoginDate);
        sqlCommand.Parameters.AddWithValue("@eventRewards", state.EventSpinRewardsToday);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously updates an existing slot machine state record for a user.
    /// </summary>
    /// <param name="state">The <see cref="UserSpinData"/> object containing the updated values.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <remarks>Uninitialized (default) DateTime fields are stored as database NULLs.</remarks>
    public async Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = "UPDATE dbo.UserSpins SET DailySpinsRemaining = @dailySpins, BonusSpins = @bonusSpins, LastSlotSpinReset = @lastSlotReset, LastTriviaSpinReset = @lastTriviaReset, LoginStreak = @loginStreak, LastLoginDate = @lastLoginDate, EventSpinRewardsToday = @eventRewards WHERE UserId = @userId";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@dailySpins", state.DailySpinsRemaining);
        sqlCommand.Parameters.AddWithValue("@bonusSpins", state.BonusSpins);
        sqlCommand.Parameters.AddWithValue("@lastSlotReset", state.LastSlotSpinReset == default ? DBNull.Value : (object)state.LastSlotSpinReset);
        sqlCommand.Parameters.AddWithValue("@lastTriviaReset", state.LastTriviaSpinReset == default ? DBNull.Value : (object)state.LastTriviaSpinReset);
        sqlCommand.Parameters.AddWithValue("@loginStreak", state.LoginStreak);
        sqlCommand.Parameters.AddWithValue("@lastLoginDate", state.LastLoginDate == default ? DBNull.Value : (object)state.LastLoginDate);
        sqlCommand.Parameters.AddWithValue("@eventRewards", state.EventSpinRewardsToday);
        sqlCommand.Parameters.AddWithValue("@userId", state.UserId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}