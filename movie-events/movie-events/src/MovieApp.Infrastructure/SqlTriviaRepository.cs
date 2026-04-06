using Microsoft.Data.SqlClient;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlTriviaRepository(DatabaseOptions databaseOptions) : ITriviaRepository
{
    private readonly string _connectionString = databaseOptions.ConnectionString;

    public async Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        List<TriviaQuestion> questions = new List<TriviaQuestion>();

        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using SqlCommand sqlCommand = new SqlCommand(TriviaSqlQueries.SelectByCategory, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@category", category);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            questions.Add(MapTriviaQuestion(sqlDataReader));
        }

        return questions;
    }

    private static TriviaQuestion MapTriviaQuestion(SqlDataReader sqlDataReader)
    {
        return new TriviaQuestion
        {
            Id            = sqlDataReader.GetInt32(0),
            QuestionText  = sqlDataReader.GetString(1),
            Category      = sqlDataReader.GetString(2),
            OptionA       = sqlDataReader.GetString(3),
            OptionB       = sqlDataReader.GetString(4),
            OptionC       = sqlDataReader.GetString(5),
            OptionD       = sqlDataReader.GetString(6),
            CorrectOption = sqlDataReader.GetString(7)[0],
            MovieId       = sqlDataReader.IsDBNull(8) ? null : sqlDataReader.GetInt32(8)
        };
    }
    public async Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(
    int movieId, int count = 3, CancellationToken cancellationToken = default)
    {
        List<TriviaQuestion> questions = new List<TriviaQuestion>();
        await using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using var sqlCommand = new SqlCommand(
            TriviaSqlQueries.SelectRandomByMovieId, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@movieId", movieId);
        sqlCommand.Parameters.AddWithValue("@count", count);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
            questions.Add(MapTriviaQuestion(sqlDataReader));

        return questions;
    }
}