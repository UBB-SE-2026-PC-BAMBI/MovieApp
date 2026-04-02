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
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.CheckReferralCodeExists, connection);
        command.Parameters.AddWithValue("@referralCode", referralCode);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)(result ?? false);
    }

    public async Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.SelectReferralCodeByUserId, connection);
        command.Parameters.AddWithValue("@userId", userId);

        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }

    public async Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.InsertAmbassadorProfile, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@referralCode", referralCode);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.SelectUserIdByReferralCode, connection);
        command.Parameters.AddWithValue("@referralCode", referralCode);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as int?;
    }

    public async Task AddReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.InsertReferralLog, connection);
        command.Parameters.AddWithValue("@ambassadorId", ambassadorId);
        command.Parameters.AddWithValue("@friendId", friendId);
        command.Parameters.AddWithValue("@eventId", eventId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.ApplyRewardIfEligible, connection);
        command.Parameters.AddWithValue("@ambassadorId", ambassadorId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)(result ?? false);
    }

    public async Task<System.Collections.Generic.IEnumerable<MovieApp.Core.Models.ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.SelectReferralHistoryByAmbassadorId, connection);
        command.Parameters.AddWithValue("@ambassadorId", ambassadorId);

        var results = new System.Collections.Generic.List<MovieApp.Core.Models.ReferralHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new MovieApp.Core.Models.ReferralHistoryItem
            {
                FriendName = reader.GetString(0),
                EventTitle = reader.GetString(1),
                UsedAt = reader.GetDateTime(2),
            });
        }

        return results;
    }

    public async Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.SelectRewardBalance, connection);
        command.Parameters.AddWithValue("@userId", userId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is int balance ? balance : 0;
    }

    public async Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ReferralSqlQueries.DecrementRewardBalance, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> HasReferralLogAsync(int ambassadorId, int friendId, int eventId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(CASE WHEN EXISTS (
                SELECT 1 FROM dbo.ReferralLog
                WHERE AmbassadorID = @ambassadorId
                  AND FriendID     = @friendId
                  AND EventID      = @eventId
            ) THEN 1 ELSE 0 END AS BIT);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ambassadorId", ambassadorId);
        command.Parameters.AddWithValue("@friendId", friendId);
        command.Parameters.AddWithValue("@eventId", eventId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)(result ?? false);
    }
}
