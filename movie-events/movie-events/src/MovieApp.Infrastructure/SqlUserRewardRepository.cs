using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// sqlStringCommand Server-backed repository for managing user discount rewards.
/// Handles access to the user_movie_discounts table.
/// </summary>
public sealed class SqlUserRewardRepository : IUserMovieDiscountRepository
{
    private readonly string _connectionString;

    public SqlUserRewardRepository(DatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);
        _connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Creates a new discount reward record for a user.
    /// </summary>
    public async Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            INSERT INTO dbo.UserMovieDiscounts (UserId, MovieId, DiscountPercentage)
            VALUES (@userId, @movieId, @discountPercentage)";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", reward.OwnerUserId);
        sqlCommand.Parameters.AddWithValue("@movieId", reward.EventId ?? 0);
        sqlCommand.Parameters.AddWithValue("@discountPercentage", (int)reward.DiscountValue);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all discount rewards associated with a user.
    /// </summary>
    public async Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            SELECT Id, UserId, MovieId, DiscountPercentage, CreatedAt
            FROM dbo.UserMovieDiscounts
            WHERE UserId = @userId
            ORDER BY CreatedAt DESC";

        List<Reward> rewards = new List<Reward>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            rewards.Add(new Reward
            {
                RewardId           = sqlDataReader.GetInt32(0),
                OwnerUserId        = sqlDataReader.GetInt32(1),
                EventId            = sqlDataReader.IsDBNull(2) ? null : sqlDataReader.GetInt32(2),
                DiscountValue      = sqlDataReader.IsDBNull(3) ? 0 : (double)sqlDataReader.GetDecimal(3),
                RewardType         = "MovieDiscount",
                RedemptionStatus   = false,
                ApplicabilityScope = "MovieSpecific"
            });
        }

        return rewards;
    }

    /// <summary>
    /// Marks the specified reward as redeemed in the database.
    /// </summary>
    public async Task MarkRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        const string sqlStringCommand = @"
            UPDATE dbo.UserMovieDiscounts
            SET IsRedeemed = 1
            WHERE Id = @rewardId";

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(sqlStringCommand, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@rewardId", rewardId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}
