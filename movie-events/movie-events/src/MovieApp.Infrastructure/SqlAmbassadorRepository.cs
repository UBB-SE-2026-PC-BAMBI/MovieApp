using Microsoft.Data.SqlClient;
using MovieApp.Core.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MovieApp.Infrastructure;

/// <summary>
/// A SQL Server implementation of <see cref="IAmbassadorRepository"/> using ADO.NET.
/// Manages ambassador profiles, referral logs, and reward balances.
/// </summary>
public sealed class SqlAmbassadorRepository : IAmbassadorRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlAmbassadorRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    public SqlAmbassadorRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously checks if a specified referral code exists in the database.
    /// </summary>
    /// <param name="referralCode">The referral code to validate.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the referral code is valid and exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsReferralCodeValidAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.CheckReferralCodeExists, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@referralCode", referralCode);

        object? isReferralCodeValid = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return (bool)(isReferralCodeValid ?? false);
    }

    /// <summary>
    /// Asynchronously retrieves the referral code associated with a specific user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The user's referral code, or <c>null</c> if not found.</returns>
    public async Task<string?> GetReferralCodeAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectReferralCodeByUserId, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        return await sqlCommand.ExecuteScalarAsync(cancellationToken) as string;
    }

    /// <summary>
    /// Asynchronously creates a new ambassador profile for a user with the provided referral code.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to become an ambassador.</param>
    /// <param name="referralCode">The new referral code to assign.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task CreateAmbassadorProfileAsync(int userId, string referralCode, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.InsertAmbassadorProfile, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);
        sqlCommand.Parameters.AddWithValue("@referralCode", referralCode);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously looks up a user's ID by their referral code.
    /// </summary>
    /// <param name="referralCode">The referral code to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The user ID associated with the referral code, or <c>null</c> if not found.</returns>
    public async Task<int?> GetUserIdByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectUserIdByReferralCode, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@referralCode", referralCode);

        object? userID = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return userID as int?;
    }

    /// <summary>
    /// Asynchronously logs a successful referral event.
    /// </summary>
    /// <param name="ambassadorId">The ID of the ambassador whose code was used.</param>
    /// <param name="friendId">The ID of the friend who used the code.</param>
    /// <param name="eventId">The ID of the event associated with the referral.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
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

    /// <summary>
    /// Asynchronously evaluates eligibility and applies a reward to an ambassador if conditions are met.
    /// </summary>
    /// <param name="ambassadorId">The ID of the ambassador to evaluate.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if a reward was successfully applied; otherwise, <c>false</c>.</returns>
    public async Task<bool> TryApplyRewardAsync(int ambassadorId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.ApplyRewardIfEligible, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@ambassadorId", ambassadorId);

        object? tryApplyReward = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return (bool)(tryApplyReward ?? false);
    }

    /// <summary>
    /// Asynchronously retrieves the referral history for a specific ambassador.
    /// </summary>
    /// <param name="ambassadorId">The ID of the ambassador.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="IEnumerable{ReferralHistoryItem}"/> representing the ambassador's successful referrals.</returns>
    public async Task<IEnumerable<MovieApp.Core.Models.ReferralHistoryItem>> GetReferralHistoryAsync(int ambassadorId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectReferralHistoryByAmbassadorId, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@ambassadorId", ambassadorId);

        List<Core.Models.ReferralHistoryItem> results = new List<MovieApp.Core.Models.ReferralHistoryItem>();
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

    /// <summary>
    /// Asynchronously gets the current available reward balance for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The user's current reward balance. Returns 0 if no balance exists.</returns>
    public async Task<int> GetRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.SelectRewardBalance, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        object? rewardBalance = await sqlCommand.ExecuteScalarAsync(cancellationToken);
        return rewardBalance is int balance ? balance : 0;
    }

    /// <summary>
    /// Asynchronously decreases a user's reward balance, typically after a reward redemption.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    public async Task DecrementRewardBalanceAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(ReferralSqlQueries.DecrementRewardBalance, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously checks if a specific referral log already exists to prevent duplicate entries.
    /// </summary>
    /// <param name="ambassadorId">The ID of the referring ambassador.</param>
    /// <param name="friendId">The ID of the referred friend.</param>
    /// <param name="eventId">The ID of the associated event.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the exact referral has already been logged; otherwise, <c>false</c>.</returns>
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