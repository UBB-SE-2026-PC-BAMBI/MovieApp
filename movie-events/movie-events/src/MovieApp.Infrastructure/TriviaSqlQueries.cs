// <copyright file="TriviaSqlQueries.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the trivia-table projections used by <see cref="SqlTriviaRepository"/>.
/// </summary>
public static class TriviaSqlQueries
{
    /// <summary>
    /// The column order here must stay aligned with <see cref="SqlTriviaRepository.MapTriviaQuestion"/>.
    /// </summary>
    public const string Projection = """
        Id, QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId
        """;

    /// <summary>
    /// Gets the SQL query used to retrieve trivia questions belonging to a specific category name.
    /// </summary>
    public const string SelectByCategory = $$"""
        SELECT {{Projection}}
        FROM dbo.TriviaQuestions
        WHERE Category = @category;
        """;

    /// <summary>
    /// Gets the SQL query used to retrieve a randomized set of trivia questions linked to a specific movie.
    /// </summary>
    public const string SelectRandomByMovieId = """
    SELECT TOP (@count) Id, QuestionText, Category,
           OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId
    FROM dbo.TriviaQuestions
    WHERE MovieId = @movieId
    ORDER BY NEWID();
    """;
}