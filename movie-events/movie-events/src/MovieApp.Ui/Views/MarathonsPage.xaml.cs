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

public sealed partial class MarathonsPage : Page
{
    private MarathonTriviaViewModel? _triviaVm;
    private int _currentMovieId;
    private int _currentUserId;

    public MarathonPageViewModel ViewModel { get; }

    private readonly IDialogService dialogService;

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
            this._currentUserId = App.Services.CurrentUserService.CurrentUser.Id;
            await this.ViewModel.LoadAsync(this._currentUserId);
        };
    }

    private async void MarathonCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (fe.Tag is not Marathon marathon) return;

        await ViewModel.SelectMarathonAsync(marathon);

        DetailTitle.Text = marathon.Title;
        DetailTheme.Text = marathon.Theme ?? string.Empty;
        LeaderboardSubtitle.Text = $"{ViewModel.Leaderboard.Count} participants";

        this.RefreshProgressBar();

        LockedBanner.Visibility = ViewModel.IsLocked
            ? Visibility.Visible : Visibility.Collapsed;
        JoinButton.Visibility = ViewModel.ShowJoinButton
            ? Visibility.Visible : Visibility.Collapsed;
        JoinPromptText.Visibility = ViewModel.ShowJoinButton
            ? Visibility.Visible : Visibility.Collapsed;


        if (ViewModel.IsJoined)
            ShowMovieList();
        else if (!ViewModel.IsLocked)
            ShowJoinPrompt();
        else
            ShowIdle();

        DetailPanel.Visibility = Visibility.Visible;
    }

    private async void JoinButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedMarathon is null) return;

        var success = await ViewModel.JoinMarathonAsync(ViewModel.SelectedMarathon.Id);

        if (success)
        {
            JoinButton.Visibility = Visibility.Collapsed;
            JoinPromptText.Visibility = Visibility.Collapsed;
            RefreshProgressBar();
            ShowMovieList();
        }
        else
        {
            LockedBanner.Visibility = Visibility.Visible;
        }
    }

    private async void LogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not int movieId) return;
        if (App.Services.TriviaRepository is null) return;

        _currentMovieId = movieId;

        var movie = ViewModel.Movies.FirstOrDefault(m => m.MovieId == movieId);
        QuizMovieTitle.Text = movie?.Title ?? "Movie";

        _triviaVm = new MarathonTriviaViewModel(App.Services.TriviaRepository);

        try
        {
            await _triviaVm.StartAsync(movieId);
            ShowPlaying();
            RefreshQuizUi();
        }
        catch (InvalidOperationException ex)
        {
            await this.dialogService.ShowInfoAsync("Cannot start quiz", ex.Message);
        }
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_triviaVm is null) return;

        var selected = new[] { OptionA, OptionB, OptionC, OptionD }
            .FirstOrDefault(r => r.IsChecked == true);

        if (selected?.Tag is not char option) return;

        _triviaVm.SubmitAnswer(option);

        foreach (var r in new[] { OptionA, OptionB, OptionC, OptionD })
            r.IsChecked = false;

        SubmitButton.IsEnabled = false;

        if (_triviaVm.IsComplete)
        {
            ShowResult();

            if (_triviaVm.IsPassed && ViewModel.SelectedMarathon is not null)
                _ = LogPassedMovieAsync(
                    ViewModel.SelectedMarathon.Id,
                    _currentMovieId,
                    _triviaVm.CorrectCount);
        }
        else
        {
            RefreshQuizUi();
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
        if (_triviaVm is null || App.Services.TriviaRepository is null) return;
        _triviaVm.Reset();
        await _triviaVm.StartAsync(_currentMovieId);
        ShowPlaying();
        RefreshQuizUi();
    }

    private void BackToMoviesButton_Click(object sender, RoutedEventArgs e)
    {
        ShowMovieList();
    }

    private void RefreshQuizUi()
    {
        if (_triviaVm?.CurrentQuestion is not TriviaQuestion q) return;

        QuizProgress.Text = _triviaVm.ProgressText;
        QuizQuestion.Text = q.QuestionText;
        OptionA.Content = q.OptionA; OptionA.Tag = 'A';
        OptionB.Content = q.OptionB; OptionB.Tag = 'B';
        OptionC.Content = q.OptionC; OptionC.Tag = 'C';
        OptionD.Content = q.OptionD; OptionD.Tag = 'D';
        SubmitButton.IsEnabled = false;

        foreach (var r in new[] { OptionA, OptionB, OptionC, OptionD })
            r.Checked += (_, _) => SubmitButton.IsEnabled = true;
    }

    private void RefreshProgressBar()
    {
        if (ViewModel.Movies.Count == 0)
        {
            ProgressBar.Value = 0;
            return;
        }
        var verified = ViewModel.Movies.Count(m => m.IsVerified);
        ProgressBar.Value = (double)verified / ViewModel.Movies.Count;
    }

    private void ShowIdle()
    {
        MovieListPanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowJoinPrompt()
    {
        MovieListPanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowMovieList()
    {
        MovieListPanel.Visibility = Visibility.Visible;
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowPlaying()
    {
        MovieListPanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Visible;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowResult()
    {
        MovieListPanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Visible;
        ResultText.Text = _triviaVm?.ResultText ?? string.Empty;
        TryAgainButton.Visibility = _triviaVm?.IsPassed == false
            ? Visibility.Visible : Visibility.Collapsed;
    }
}