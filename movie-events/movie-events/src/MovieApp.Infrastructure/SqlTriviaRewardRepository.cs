using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlTriviaRewardRepository(DatabaseOptions databaseOptions) : ITriviaRewardRepository
{
    private readonly string _connectionString = databaseOptions.ConnectionString;

    public async Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(TriviaRewardSqlQueries.Insert, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@userId", reward.UserId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
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

    public async Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(TriviaRewardSqlQueries.MarkAsRedeemed, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@rewardId", rewardId);

        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static TriviaReward MapTriviaReward(SqlDataReader sqlDataReader)
    {
        return new TriviaReward
        {
            Id         = sqlDataReader.GetInt32(0),
            UserId     = sqlDataReader.GetInt32(1),
            IsRedeemed = sqlDataReader.GetBoolean(2),
            CreatedAt  = sqlDataReader.GetDateTime(3)
        };
    }
}