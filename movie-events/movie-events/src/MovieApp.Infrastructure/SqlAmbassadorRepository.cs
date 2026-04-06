using Microsoft.Data.SqlClient;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlAmbassadorRepository : IAmbassadorRepository
{
    private readonly string _connectionString;

    public SqlAmbassadorRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.CheckReferralCodeExists, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@referralCode", referralCode);

        object? isReferralCodeValid = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return (bool)(isReferralCodeValid ?? false);
    }

    public async Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectReferralCodeByUserId, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        return await sqlCommand.ExecuteScalarAsync(cancellationToken) as string;
    }

    public async Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.InsertAmbassadorProfile, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@referralCode", referralCode);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectUserIdByReferralCode, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@referralCode", referralCode);

        object? userID = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return userID as int?;
    }

    public async Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.InsertReferralLog, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@ambassadorId", ambassadorId);
        sqlCommand.Parameters.AddWithValue("@friendId", friendId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.ApplyRewardIfEligible, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@ambassadorId", ambassadorId);

        object? tryApplyReward = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return (bool)(tryApplyReward ?? false);
    }

    public async Task<System.Collections.Generic.IEnumerable<MovieApp.Core.Models.ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectReferralHistoryByAmbassadorId, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@ambassadorId", ambassadorId);

        List<Core.Models.ReferralHistoryItem> results = new System.Collections.Generic.List<MovieApp.Core.Models.ReferralHistoryItem>();
        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            results.Add(new MovieApp.Core.Models.ReferralHistoryItem
            {
                FriendName = sqlDataReader.GetString(0),
                EventTitle = sqlDataReader.GetString(1),
                UsedAt = sqlDataReader.GetDateTime(2),
            });
        }

        return results;
    }

    public async Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectRewardBalance, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        object? rewardBalance = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return rewardBalance is int balance ? balance : 0;
    }

    public async Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.DecrementRewardBalance, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = """
            SELECT CAST(CASE WHEN EXISTS (
                SELECT 1 FROM dbo.ReferralLog
                WHERE AmbassadorID = @ambassadorId
                  AND FriendID     = @friendId
                  AND EventID      = @eventId
            ) THEN 1 ELSE 0 END AS BIT);
            """;

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@ambassadorId", ambassadorId);
        sqlCommand.Parameters.AddWithValue("@friendId", friendId);
        sqlCommand.Parameters.AddWithValue("@eventId", eventId);

        object? hasReferralLog = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return (bool)(hasReferralLog ?? false);
    }
}
