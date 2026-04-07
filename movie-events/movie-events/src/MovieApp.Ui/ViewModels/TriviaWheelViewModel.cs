// <copyright file="TriviaWheelViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// ViewModel responsible for managing the trivia wheel gameplay logic.
/// </summary>
public sealed class TriviaWheelViewModel : ViewModelBase
{
    private const int QuestionsPerSession = 20;
    private const string ErrorMessage = "Trivia unavailable: no trivia data in the database.";

    private readonly ITriviaRepository triviaRepository;
    private readonly ITriviaRewardRepository triviaRewardRepository;
    private readonly IUserSlotMachineStateRepository spinRepository;
    private readonly int currentUserId;

    private List<TriviaQuestion> questions = new ();
    private UserSpinData? spinData;

    private bool disableDailySpinLimit = false;
    private string selectedCategory = string.Empty;
    private bool canSpin = true;
    private bool isPlaying;
    private bool isSessionComplete;
    private TriviaQuestion? currentQuestion;
    private int currentQuestionIndex;
    private int score;
    private bool hintUsed;
    private bool noQuestionsAvailable;
    private bool isTriviaAvailable = true;
    private string availabilityMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TriviaWheelViewModel"/> class.
    /// </summary>
    /// <param name="triviaRepository">Provides access to trivia questions.</param>
    /// <param name="triviaRewardRepository">Handles persistence of trivia rewards.</param>
    /// <param name="spinRepository">Manages user spin state data.</param>
    /// <param name="currentUserId">The identifier of the current user.</param>
    public TriviaWheelViewModel(
        ITriviaRepository triviaRepository,
        ITriviaRewardRepository triviaRewardRepository,
        IUserSlotMachineStateRepository spinRepository,
        int currentUserId)
    {
        this.triviaRepository = triviaRepository;
        this.triviaRewardRepository = triviaRewardRepository;
        this.spinRepository = spinRepository;
        this.currentUserId = currentUserId;
    }

    /// <summary>
    /// Gets the available trivia categories.
    /// </summary>
    public IReadOnlyList<string> Categories { get; } = new List<string>
    {
        "Actors",
        "Directors",
        "Movie Quotes",
        "Oscars and Awards",
        "General Movie Trivia",
    };

    /// <summary>
    /// Gets a value indicating whether the user can spin.
    /// </summary>
    public bool CanSpin
    {
        get => this.canSpin;
        private set
        {
            this.canSpin = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.RemainingSpinsText));
        }
    }

    /// <summary>
    /// Gets the remaining spins text.
    /// </summary>
    public string RemainingSpinsText => this.CanSpin
        ? "1 spin available today"
        : "Next spin available tomorrow";

    /// <summary>
    /// Gets the selected category.
    /// </summary>
    public string SelectedCategory
    {
        get => this.selectedCategory;
        private set
        {
            this.selectedCategory = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the game is currently active.
    /// </summary>
    public bool IsPlaying
    {
        get => this.isPlaying;
        private set
        {
            this.isPlaying = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the session is complete.
    /// </summary>
    public bool IsSessionComplete
    {
        get => this.isSessionComplete;
        private set
        {
            this.isSessionComplete = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.HasEarnedReward));
        }
    }

    /// <summary>
    /// Gets the current question.
    /// </summary>
    public TriviaQuestion? CurrentQuestion
    {
        get => this.currentQuestion;
        private set
        {
            this.currentQuestion = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the current question index.
    /// </summary>
    public int CurrentQuestionIndex
    {
        get => this.currentQuestionIndex;
        private set
        {
            this.currentQuestionIndex = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.ProgressValue));
            this.OnPropertyChanged(nameof(this.ProgressText));
        }
    }

    /// <summary>
    /// Gets the score.
    /// </summary>
    public int Score
    {
        get => this.score;
        private set
        {
            this.score = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a hint was used.
    /// </summary>
    public bool HintUsed
    {
        get => this.hintUsed;
        private set
        {
            this.hintUsed = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.IsHintAvailable));
        }
    }

    /// <summary>
    /// Gets a value indicating whether a hint is available.
    /// </summary>
    public bool IsHintAvailable => !this.HintUsed;

    /// <summary>
    /// Gets a value indicating whether the user earned a reward.
    /// </summary>
    public bool HasEarnedReward => this.Score == QuestionsPerSession;

    /// <summary>
    /// Gets the progress value.
    /// </summary>
    public double ProgressValue => (double)this.CurrentQuestionIndex / QuestionsPerSession * 100;

    /// <summary>
    /// Gets the progress text.
    /// </summary>
    public string ProgressText => $"{this.CurrentQuestionIndex}/{QuestionsPerSession}";

    /// <summary>
    /// Gets a value indicating whether trivia is available.
    /// </summary>
    public bool IsTriviaAvailable
    {
        get => this.isTriviaAvailable;
        private set
        {
            this.isTriviaAvailable = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the availability message.
    /// </summary>
    public string AvailabilityMessage
    {
        get => this.availabilityMessage;
        private set
        {
            this.availabilityMessage = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether no questions are available.
    /// </summary>
    public bool NoQuestionsAvailable
    {
        get => this.noQuestionsAvailable;
        private set
        {
            this.noQuestionsAvailable = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.EmptyStateMessage));
        }
    }

    /// <summary>
    /// Gets the empty state message.
    /// </summary>
    public string EmptyStateMessage => this.NoQuestionsAvailable
        ? "No trivia questions are available for this category yet."
        : "Spin the wheel to begin!";

    /// <summary>
    /// Initializes the trivia state for the current user, including spin availability and trivia data.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task InitializeAsync()
    {
        if (this.disableDailySpinLimit)
        {
            this.CanSpin = true;
            await this.RefreshTriviaAvailabilityAsync();
            return;
        }

        try
        {
            this.spinData = await this.spinRepository.GetByUserIdAsync(this.currentUserId);

            if (this.spinData is null)
            {
                this.spinData = new UserSpinData
                {
                    UserId = this.currentUserId,
                    DailySpinsRemaining = 1,
                    BonusSpins = 0,
                    LoginStreak = 0,
                    EventSpinRewardsToday = 0,
                };

                await this.spinRepository.CreateAsync(this.spinData);
                this.CanSpin = true;
            }
            else
            {
                this.CanSpin = !HasSpunToday(this.spinData.LastTriviaSpinReset);
            }

            await this.RefreshTriviaAvailabilityAsync();
        }
        catch
        {
            this.CanSpin = false;
            this.IsTriviaAvailable = false;
            this.AvailabilityMessage = "Trivia unavailable: no database connection.";
        }
    }

    /// <summary>
    /// Records a trivia spin for the current user and updates the spin state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RecordSpinAsync()
    {
        if (this.disableDailySpinLimit)
        {
            return;
        }

        this.spinData ??= await this.spinRepository.GetByUserIdAsync(this.currentUserId);

        if (this.spinData is null)
        {
            return;
        }

        this.spinData.LastTriviaSpinReset = DateTime.UtcNow;
        await this.spinRepository.UpdateAsync(this.spinData);
        this.CanSpin = false;
    }

    /// <summary>
    /// Loads and prepares trivia questions for the specified category.
    /// </summary>
    /// <param name="category">The category from which to retrieve trivia questions.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoadQuestionsAsync(string category)
    {
        this.SelectedCategory = category;

        IEnumerable<TriviaQuestion> all = await this.triviaRepository.GetByCategoryAsync(category);

        this.questions = all
            .OrderBy(_ => Guid.NewGuid())
            .Take(QuestionsPerSession)
            .ToList();

        this.CurrentQuestionIndex = 0;
        this.Score = 0;
        this.HintUsed = false;
        this.IsSessionComplete = false;
        this.NoQuestionsAvailable = false;
        this.CurrentQuestion = null;

        if (this.questions.Count < QuestionsPerSession)
        {
            this.IsPlaying = false;
            this.NoQuestionsAvailable = true;
            return;
        }

        this.IsPlaying = true;
        this.AdvanceToNextQuestion();
    }

    /// <summary>
    /// Submits the selected answer for the current trivia question and updates the game state.
    /// </summary>
    /// <param name="selectedOption">The selected answer option.</param>
    public void SubmitAnswer(char selectedOption)
    {
        if (this.CurrentQuestion is null)
        {
            return;
        }

        if (selectedOption == this.CurrentQuestion.CorrectOption)
        {
            this.Score++;
        }

        if (this.CurrentQuestionIndex >= this.questions.Count)
        {
            this.IsSessionComplete = true;
            this.IsPlaying = false;

            if (this.HasEarnedReward)
            {
                _ = this.GrantRewardAsync();
            }
        }
        else
        {
            this.AdvanceToNextQuestion();
        }
    }

    /// <summary>
    /// Uses a hint.
    /// </summary>
    public void UseHint()
    {
        this.HintUsed = true;
    }

    /// <summary>
    /// Gets a collection of answer options that should be hidden when using a hint.
    /// </summary>
    /// <returns>A read-only list of answer options to hide.</returns>
    public IReadOnlyList<char> GetHintOptionsToHide()
    {
        if (this.CurrentQuestion is null)
        {
            return Array.Empty<char>();
        }

        return new List<char> { 'A', 'B', 'C', 'D' }
            .Where(o => o != this.CurrentQuestion.CorrectOption)
            .OrderBy(_ => Guid.NewGuid())
            .Take(2)
            .ToList();
    }

    private static bool HasSpunToday(DateTime lastSpin)
    {
        if (lastSpin == default)
        {
            return false;
        }

        return lastSpin.Date == DateTime.UtcNow.Date;
    }

    private async Task RefreshTriviaAvailabilityAsync()
    {
        foreach (string category in this.Categories)
        {
            IEnumerable<TriviaQuestion> questions = await this.triviaRepository.GetByCategoryAsync(category);

            if (questions.Any())
            {
                this.IsTriviaAvailable = true;
                this.AvailabilityMessage = string.Empty;
                return;
            }
        }

        this.IsTriviaAvailable = false;
        this.AvailabilityMessage = ErrorMessage;
        this.CanSpin = false;
    }

    private async Task GrantRewardAsync()
    {
        TriviaReward reward = new ()
        {
            Id = 0,
            UserId = this.currentUserId,
            IsRedeemed = false,
            CreatedAt = DateTime.UtcNow,
        };

        await this.triviaRewardRepository.AddAsync(reward);
    }

    private void AdvanceToNextQuestion()
    {
        this.CurrentQuestion = this.questions[this.CurrentQuestionIndex];
        this.CurrentQuestionIndex++;
    }

    private void FlipDisableDailySpinLimit()
    {
        this.disableDailySpinLimit = !this.disableDailySpinLimit;
    }
}
