// <copyright file="TriviaQuestion.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

/// <summary>
/// Represents a persisted trivia question together with its answer choices.
/// </summary>
public sealed class TriviaQuestion
{
    /// <summary>
    /// Gets the persistent question identifier.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the prompt shown to the user.
    /// </summary>
    required public string QuestionText { get; init; }

    /// <summary>
    /// Gets the trivia category the question belongs to.
    /// </summary>
    required public string Category { get; init; }

    /// <summary>
    /// Gets the first answer choice.
    /// </summary>
    required public string OptionA { get; init; }

    /// <summary>
    /// Gets the second answer choice.
    /// </summary>
    required public string OptionB { get; init; }

    /// <summary>
    /// Gets the third answer choice.
    /// </summary>
    required public string OptionC { get; init; }

    /// <summary>
    /// Gets the fourth answer choice.
    /// </summary>
    required public string OptionD { get; init; }

    /// <summary>
    /// Gets the option key of the correct answer.
    /// </summary>
    required public char CorrectOption { get; init; }

    /// <summary>
    /// Gets the related movie identifier when the question belongs to a movie-specific pool.
    /// </summary>
    public int? MovieId { get; init; }
}
