namespace MovieApp.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// A SQL Server-backed repository for managing user trivia rewards via ADO.NET.
/// </summary>
/// <param name="databaseOptions">The database options containing the SQL connection string.</param>
public sealed class SqlTriviaRewardRepository(DatabaseOptions databaseOptions) : ITriviaRewardRepository
{
    private readonly string connectionString = databaseOptions.ConnectionString;

    /// <summary>
    /// Asynchronously inserts a new trivia reward for a user into the database.
    /// </summary>
    /// <param name="reward">The <see cref="TriviaReward"/> object containing the target User ID.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>Returns a Task.</returns>
    public async Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(TriviaRewardSqlQueries.Insert, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", reward.UserId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves an unredeemed trivia reward for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The unredeemed <see cref="TriviaReward"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(TriviaRewardSqlQueries.SelectUnredeemedByUser, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", userId);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        if (!await sqlDataReader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapTriviaReward(sqlDataReader);
    }

    /// <summary>
    /// Asynchronously updates a specific trivia reward to mark it as redeemed.
    /// </summary>
    /// <param name="rewardId">The unique identifier of the reward to update.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>Returns a Task.</returns>
    public async Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(TriviaRewardSqlQueries.MarkAsRedeemed, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@rewardId", rewardId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Maps the current row of a <see cref="SqlDataReader"/> to a <see cref="TriviaReward"/> object.
    /// </summary>
    /// <param name="sqlDataReader">The data reader currently positioned on a valid row.</param>
    /// <returns>A populated <see cref="TriviaReward"/>.</returns>
    private static TriviaReward MapTriviaReward(SqlDataReader sqlDataReader)
    {
        return new TriviaReward
        {
            Id = sqlDataReader.GetInt32(0),
            UserId = sqlDataReader.GetInt32(1),
            IsRedeemed = sqlDataReader.GetBoolean(2),
            CreatedAt = sqlDataReader.GetDateTime(3),
        };
    }
}