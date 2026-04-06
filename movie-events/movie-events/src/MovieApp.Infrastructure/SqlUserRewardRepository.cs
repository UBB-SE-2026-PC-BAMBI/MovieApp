namespace MovieApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// A SQL Server-backed repository for managing user discount rewards.
/// Handles data access to the dbo.UserMovieDiscounts table via ADO.NET.
/// </summary>
public sealed class SqlUserRewardRepository : IUserMovieDiscountRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlUserRewardRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">The database options containing the SQL connection string.</param>
    /// <exception cref="ArgumentNullException">Thrown if the databaseOptions parameter is null.</exception>
    public SqlUserRewardRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously creates a new discount reward record for a user.
    /// </summary>
    /// <param name="reward">The <see cref="Reward"/> entity containing the discount details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>Returns a Task.</returns>
    public async Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            INSERT INTO dbo.UserMovieDiscounts (UserId, MovieId, DiscountPercentage)
            VALUES (@userId, @movieId, @discountPercentage)";

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", reward.OwnerUserId);
        sqlCommand.Parameters.AddWithValue("@movieId", reward.EventId ?? 0);
        sqlCommand.Parameters.AddWithValue("@discountPercentage", (int)reward.DiscountValue);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves all discount rewards associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A list of <see cref="Reward"/> objects, ordered from newest to oldest.</returns>
    public async Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT Id, UserId, MovieId, DiscountPercentage, CreatedAt
            FROM dbo.UserMovieDiscounts
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC";

        List<Reward> rewards = new List<Reward>();

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            rewards.Add(new Reward
            {
                RewardId = sqlDataReader.GetInt32(0),
                OwnerUserId = sqlDataReader.GetInt32(1),
                EventId = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetInt32(2),
                DiscountValue = sqlDataReader.IsDBNull(3) ? 0 : (double)sqlDataReader.GetDecimal(3),
                RewardType = "MovieDiscount",
                RedemptionStatus = false,
                ApplicabilityScope = "MovieSpecific",
            });
        }

        return rewards;
    }

    /// <summary>
    /// Asynchronously marks the specified reward as redeemed in the database.
    /// </summary>
    /// <param name="rewardId">The unique identifier of the reward to mark as redeemed.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task MarkRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            UPDATE dbo.UserMovieDiscounts
            SET IsRedeemed = 1
            WHERE Id = @rewardId";

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@rewardId", rewardId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}