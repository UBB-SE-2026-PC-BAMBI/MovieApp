// <copyright file="SqlTriviaRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Infrastructure;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// A SQL Server-backed repository for managing and retrieving movie trivia questions via ADO.NET.
/// </summary>
/// <param name="databaseOptions">The database options containing the SQL connection string.</param>
public sealed class SqlTriviaRepository : ITriviaRepository
{
    private readonly string connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlTriviaRepository"/> class.
    /// </summary>
    /// <param name="databaseOptions">DataBase options.</param>
    public SqlTriviaRepository(DatabaseOptions databaseOptions)
    {
        this.connectionString = databaseOptions.ConnectionString;
    }

    /// <summary>
    /// Asynchronously retrieves a list of trivia questions filtered by a specific category.
    /// </summary>
    /// <param name="category">The category name to filter questions by.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IEnumerable{TriviaQuestion}"/> containing the matching questions.</returns>
    public async Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        List<TriviaQuestion> questions = new List<TriviaQuestion>();

        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
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

    /// <summary>
    /// Asynchronously retrieves a random selection of trivia questions for a specific movie.
    /// </summary>
    /// <param name="movieId">The unique identifier of the movie.</param>
    /// <param name="count">The maximum number of random questions to return. Defaults to 3.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IEnumerable{TriviaQuestion}"/> containing the randomly selected questions.</returns>
    public async Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(
    int movieId, int count = 3, CancellationToken cancellationToken = default)
    {
        List<TriviaQuestion> questions = new List<TriviaQuestion>();
        await using SqlConnection sqlConnection = new SqlConnection(this.connectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        await using var sqlCommand = new SqlCommand(
            TriviaSqlQueries.SelectRandomByMovieId, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@movieId", movieId);
        sqlCommand.Parameters.AddWithValue("@count", count);

        await using SqlDataReader sqlDataReader = await sqlCommand.ExecuteReaderAsync(cancellationToken);
        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            questions.Add(MapTriviaQuestion(sqlDataReader));
        }

        return questions;
    }

    /// <summary>
    /// Maps the current row of a <see cref="SqlDataReader"/> to a <see cref="TriviaQuestion"/> object.
    /// </summary>
    /// <param name="sqlDataReader">The data reader currently positioned on a valid row.</param>
    /// <returns>A populated <see cref="TriviaQuestion"/>.</returns>
    private static TriviaQuestion MapTriviaQuestion(SqlDataReader sqlDataReader)
    {
        return new TriviaQuestion
        {
            Id = sqlDataReader.GetInt32(0),
            QuestionText = sqlDataReader.GetString(1),
            Category = sqlDataReader.GetString(2),
            OptionA = sqlDataReader.GetString(3),
            OptionB = sqlDataReader.GetString(4),
            OptionC = sqlDataReader.GetString(5),
            OptionD = sqlDataReader.GetString(6),
            CorrectOption = sqlDataReader.GetString(7)[0],
            MovieId = sqlDataReader.IsDBNull(8) ? null : sqlDataReader.GetInt32(8),
        };
    }
}