// <copyright file="MarathonsPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MovieApp.Core.Models;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Represents the marathons page, allowing users to browse marathons,
/// join them, and complete trivia challenges for movies.
/// </summary>
public sealed partial class MarathonsPage : Page
{
    private readonly IDialogService dialogService;

    private MarathonTriviaViewModel? triviaVm;
    private int currentMovieId;
    private int currentUserId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarathonsPage"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required services are not configured or the user session is invalid.
    /// </exception>
    public MarathonsPage()
    {
        if (App.Services.MarathonService == null)
        {
            throw new InvalidOperationException("Marathon service is not configured.");
        }

        if (App.Services.DialogService == null)
        {
            throw new InvalidOperationException("Dialog service is not configured.");
        }

        this.ViewModel = new MarathonPageViewModel(App.Services.MarathonService);
        this.dialogService = App.Services.DialogService;
        this.InitializeComponent();

        this.Loaded += async (_, _) =>
        {
            App.EnsureServicesValid();

            if (App.Services.CurrentUserService?.CurrentUser == null)
            {
                throw new InvalidOperationException("User session is invalid or has expired.");
            }

            this.dialogService.SetXamlRoot(this.XamlRoot);
            this.currentUserId = App.Services.CurrentUserService.CurrentUser.Id;
            await this.ViewModel.LoadAsync(this.currentUserId);
        };
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public MarathonPageViewModel ViewModel { get; }

    private async void MarathonCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement frameworkElement)
        {
            return;
        }

        if (frameworkElement.Tag is not Marathon marathon)
        {
            return;
        }

        await this.ViewModel.SelectMarathonAsync(marathon);

        this.DetailTitle.Text = marathon.Title;
        this.DetailTheme.Text = marathon.Theme ?? string.Empty;
        this.LeaderboardSubtitle.Text = $"{this.ViewModel.Leaderboard.Count} participants";

        this.RefreshProgressBar();

        this.LockedBanner.Visibility = this.ViewModel.IsLocked
            ? Visibility.Visible : Visibility.Collapsed;
        this.JoinButton.Visibility = this.ViewModel.ShowJoinButton
            ? Visibility.Visible : Visibility.Collapsed;
        this.JoinPromptText.Visibility = this.ViewModel.ShowJoinButton
            ? Visibility.Visible : Visibility.Collapsed;

        if (this.ViewModel.IsJoined)
        {
            this.ShowMovieList();
        }
        else if (!this.ViewModel.IsLocked)
        {
            this.ShowJoinPrompt();
        }
        else
        {
            this.ShowIdle();
        }

        this.DetailPanel.Visibility = Visibility.Visible;
    }

    private async void JoinButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.ViewModel.SelectedMarathon is null)
        {
            return;
        }

        bool success = await this.ViewModel.JoinMarathonAsync(this.ViewModel.SelectedMarathon.Id);

        if (success)
        {
            this.JoinButton.Visibility = Visibility.Collapsed;
            this.JoinPromptText.Visibility = Visibility.Collapsed;
            this.RefreshProgressBar();
            this.ShowMovieList();
        }
        else
        {
            this.LockedBanner.Visibility = Visibility.Visible;
        }
    }

    private async void LogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not int movieId)
        {
            return;
        }

        if (App.Services.TriviaRepository is null)
        {
            return;
        }

        this.currentMovieId = movieId;

        MarathonMovieItem? movie = this.ViewModel.Movies.FirstOrDefault(m => m.MovieId == movieId);
        this.QuizMovieTitle.Text = movie?.Title ?? "Movie";

        this.triviaVm = new MarathonTriviaViewModel(App.Services.TriviaRepository);

        try
        {
            await this.triviaVm.StartAsync(movieId);
            this.ShowPlaying();
            this.RefreshQuizUi();
        }
        catch (InvalidOperationException ex)
        {
            await this.dialogService.ShowInfoAsync("Cannot start quiz", ex.Message);
        }
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.triviaVm is null)
        {
            return;
        }

        RadioButton? selected = new[] { this.OptionA, this.OptionB, this.OptionC, this.OptionD }
            .FirstOrDefault(r => r.IsChecked == true);

        if (selected?.Tag is not char option)
        {
            return;
        }

        this.triviaVm.SubmitAnswer(option);

        foreach (RadioButton radioButton in new[] { this.OptionA, this.OptionB, this.OptionC, this.OptionD })
        {
            radioButton.IsChecked = false;
        }

        this.SubmitButton.IsEnabled = false;

        if (this.triviaVm.IsComplete)
        {
            this.ShowResult();

            if (this.triviaVm.IsPassed && this.ViewModel.SelectedMarathon is not null)
            {
                _ = this.LogPassedMovieAsync(
                    this.ViewModel.SelectedMarathon.Id,
                    this.currentMovieId,
                    this.triviaVm.CorrectCount);
            }
        }
        else
        {
            this.RefreshQuizUi();
        }
    }

    private async Task LogPassedMovieAsync(int marathonId, int movieId, int correctCount)
    {
        await this.ViewModel.LogMovieAsync(marathonId, movieId, correctCount);
        await this.ViewModel.RefreshAfterMovieLoggedAsync();

        this.LeaderboardSubtitle.Text = $"{this.ViewModel.Leaderboard.Count} participants";
        this.RefreshProgressBar();
    }

    private async void TryAgainButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.triviaVm is null || App.Services.TriviaRepository is null)
        {
            return;
        }

        this.triviaVm.Reset();
        await this.triviaVm.StartAsync(this.currentMovieId);
        this.ShowPlaying();
        this.RefreshQuizUi();
    }

    private void BackToMoviesButton_Click(object sender, RoutedEventArgs e)
    {
        this.ShowMovieList();
    }

    private void RefreshQuizUi()
    {
        if (this.triviaVm?.CurrentQuestion is not TriviaQuestion q)
        {
            return;
        }

        this.QuizProgress.Text = this.triviaVm.ProgressText;
        this.QuizQuestion.Text = q.QuestionText;
        this.OptionA.Content = q.OptionA;
        this.OptionA.Tag = 'A';
        this.OptionB.Content = q.OptionB;
        this.OptionB.Tag = 'B';
        this.OptionC.Content = q.OptionC;
        this.OptionC.Tag = 'C';
        this.OptionD.Content = q.OptionD;
        this.OptionD.Tag = 'D';
        this.SubmitButton.IsEnabled = false;

        foreach (RadioButton radioButton in new[] { this.OptionA, this.OptionB, this.OptionC, this.OptionD })
        {
            radioButton.Checked += (_, _) => this.SubmitButton.IsEnabled = true;
        }
    }

    private void RefreshProgressBar()
    {
        if (this.ViewModel.Movies.Count == 0)
        {
            this.ProgressBar.Value = 0;
            return;
        }

        int verified = this.ViewModel.Movies.Count(m => m.IsVerified);
        this.ProgressBar.Value = (double)verified / this.ViewModel.Movies.Count;
    }

    private void ShowIdle()
    {
        this.MovieListPanel.Visibility = Visibility.Collapsed;
        this.PlayingPanel.Visibility = Visibility.Collapsed;
        this.ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowJoinPrompt()
    {
        this.MovieListPanel.Visibility = Visibility.Collapsed;
        this.PlayingPanel.Visibility = Visibility.Collapsed;
        this.ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowMovieList()
    {
        this.MovieListPanel.Visibility = Visibility.Visible;
        this.PlayingPanel.Visibility = Visibility.Collapsed;
        this.ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowPlaying()
    {
        this.MovieListPanel.Visibility = Visibility.Collapsed;
        this.PlayingPanel.Visibility = Visibility.Visible;
        this.ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowResult()
    {
        this.MovieListPanel.Visibility = Visibility.Collapsed;
        this.PlayingPanel.Visibility = Visibility.Collapsed;
        this.ResultPanel.Visibility = Visibility.Visible;
        this.ResultText.Text = this.triviaVm?.ResultText ?? string.Empty;
        this.TryAgainButton.Visibility = this.triviaVm?.IsPassed == false
            ? Visibility.Visible : Visibility.Collapsed;
    }
}