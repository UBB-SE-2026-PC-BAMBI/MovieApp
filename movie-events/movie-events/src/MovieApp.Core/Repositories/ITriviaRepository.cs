// <copyright file="ITriviaRepository.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Repositories;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;

/// <summary>
/// Provides persistence operations for retrieving trivia questions.
/// </summary>
public interface ITriviaRepository
{
    /// <summary>
    /// The default number of questions to retrieve for a movie-specific quiz.
    /// </summary>
    public const int DefaultQuestionCount = 3;

    /// <summary>
    /// Retrieves trivia questions belonging to a specific category.
    /// </summary>
    /// <param name="categoryName">The name of the trivia category.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of trivia questions matching the category.</returns>
    Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string categoryName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific number of trivia questions linked to a movie.
    /// </summary>
    /// <param name="movieIdentifier">The unique identifier of the movie.</param>
    /// <param name="questionCount">The number of questions to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of movie-specific trivia questions.</returns>
    Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieIdentifier, int questionCount = DefaultQuestionCount, CancellationToken cancellationToken = default);
}
