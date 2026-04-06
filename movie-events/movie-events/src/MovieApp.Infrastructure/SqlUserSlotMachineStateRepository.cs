using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// sqlStringCommand Server-backed repository for user slot-machine state. Uses the existing
/// dbo.UserSpins table and maps to <see cref="UserSpinData"/>.
/// </summary>
public sealed class SqlUserSlotMachineStateRepository : IUserSlotMachineStateRepository
{
    private readonly string _connectionString;

    public SqlUserSlotMachineStateRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);

        _connectionString = databaseOptions.ConnectionString;
    }

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
            UserId                = sqlDataReader.GetInt32(0),
            DailySpinsRemaining   = sqlDataReader.GetInt32(1),
            BonusSpins            = sqlDataReader.GetInt32(2),
            LastSlotSpinReset     = sqlDataReader.IsDBNull(3) ? default : sqlDataReader.GetDateTime(3),
            LastTriviaSpinReset   = sqlDataReader.IsDBNull(4) ? default : sqlDataReader.GetDateTime(4),
            LoginStreak           = sqlDataReader.GetInt32(5),
            LastLoginDate         = sqlDataReader.IsDBNull(6) ? default : sqlDataReader.GetDateTime(6),
            EventSpinRewardsToday = sqlDataReader.GetInt32(7)
        };
    }

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
