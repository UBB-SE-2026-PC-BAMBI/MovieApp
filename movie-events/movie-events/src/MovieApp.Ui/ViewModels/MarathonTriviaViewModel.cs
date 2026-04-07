// <copyright file="MarathonTriviaViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Drives the rapid-fire trivia check used to verify movie watches inside marathons.
/// </summary>
public sealed class MarathonTriviaViewModel : ViewModelBase
{
    private readonly ITriviaRepository triviaRepository;
    private List<TriviaQuestion> questions = new ();
    private int currentIndex;
    private int correctCount;
    private bool isLoading;
    private bool isPlaying;
    private bool isComplete;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarathonTriviaViewModel"/> class.
    /// Creates the view model with access to movie-specific trivia data.
    /// </summary>
    /// <param name="triviaRepository">The repository used to retrieve trivia questions.</param>
    public MarathonTriviaViewModel(ITriviaRepository triviaRepository)
    {
        this.triviaRepository = triviaRepository;
    }

    /// <summary>
    /// Gets a value indicating whether trivia questions are currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set => this.SetProperty(ref this.isLoading, value);
    }

    /// <summary>
    /// Gets a value indicating whether a trivia session is currently in progress.
    /// </summary>
    public bool IsPlaying
    {
        get => this.isPlaying;
        private set => this.SetProperty(ref this.isPlaying, value);
    }

    /// <summary>
    /// Gets a value indicating whether the trivia session has completed.
    /// </summary>
    public bool IsComplete
    {
        get => this.isComplete;
        private set
        {
            this.SetProperty(ref this.isComplete, value);
            this.OnPropertyChanged(nameof(this.IsPassed));
            this.OnPropertyChanged(nameof(this.ResultText));
        }
    }

    /// <summary>
    /// Gets the number of correctly answered questions in the current session.
    /// </summary>
    public int CorrectCount => this.correctCount;

    /// <summary>
    /// Gets a value indicating whether the user passed the trivia session.
    /// </summary>
    public bool IsPassed => this.IsComplete && this.correctCount == this.questions.Count;

    /// <summary>
    /// Gets the current trivia question being presented.
    /// </summary>
    public TriviaQuestion? CurrentQuestion =>
        this.currentIndex < this.questions.Count ? this.questions[this.currentIndex] : null;

    /// <summary>
    /// Gets the text describing the user's progress through the trivia session.
    /// </summary>
    public string ProgressText => this.questions.Count == 0
        ? string.Empty
        : $"Question {this.currentIndex + 1} of {this.questions.Count}";

    /// <summary>
    /// Gets the result message displayed after the trivia session completes.
    /// </summary>
    public string ResultText => !this.IsComplete
        ? string.Empty
        : this.IsPassed
            ? "Passed! Movie verified."
            : $"Failed — {this.correctCount}/{this.questions.Count} correct. Try again.";

    /// <summary>
    /// Starts a new three-question trivia verification session for the specified movie.
    /// </summary>
    /// <param name="movieId">The identifier of the movie.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync(int movieId)
    {
        this.IsLoading = true;
        this.currentIndex = 0;
        this.correctCount = 0;
        this.IsComplete = false;

        try
        {
            IEnumerable<TriviaQuestion> fetched =
                await this.triviaRepository.GetByMovieIdAsync(movieId, 3);
            this.questions = fetched.ToList();

            if (this.questions.Count < 3)
            {
                throw new InvalidOperationException(
                    $"Not enough trivia questions for movie {movieId}.");
            }

            this.IsPlaying = true;
        }
        finally
        {
            this.IsLoading = false;
        }

        this.NotifyQuestionChanged();
    }

    /// <summary>
    /// Submits an answer for the current question and advances the verification session.
    /// </summary>
    /// <param name="selected">The selected answer option.</param>
    public void SubmitAnswer(char selected)
    {
        if (!this.IsPlaying || this.CurrentQuestion is null)
        {
            return;
        }

        if (selected == this.CurrentQuestion.CorrectOption)
        {
            this.correctCount++;
        }

        this.currentIndex++;

        if (this.currentIndex >= this.questions.Count)
        {
            this.IsPlaying = false;
            this.IsComplete = true;
        }

        this.NotifyQuestionChanged();
    }

    /// <summary>
    /// Clears the current verification session state.
    /// </summary>
    public void Reset()
    {
        this.questions.Clear();
        this.currentIndex = 0;
        this.correctCount = 0;
        this.IsPlaying = false;
        this.IsComplete = false;
        this.NotifyQuestionChanged();
    }

    /// <summary>
    /// Raises property change notifications for question-related UI bindings.
    /// </summary>
    private void NotifyQuestionChanged()
    {
        this.OnPropertyChanged(nameof(this.CurrentQuestion));
        this.OnPropertyChanged(nameof(this.ProgressText));
        this.OnPropertyChanged(nameof(this.ResultText));
    }
}
