// <copyright file="TriviaWheelPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml;
using MovieApp.Ui.ViewModels;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using Windows.UI;
using Windows.Foundation;
using MovieApp.Core.Models;

/// <summary>
/// Represents the trivia wheel page, allowing users to spin a wheel,
/// answer trivia questions, and earn rewards.
/// </summary>
public sealed partial class TriviaWheelPage : Page
{
    private readonly string[] categories = new[]
    {
        "Actors", "Directors", "Movie Quotes", "Oscars and Awards", "General Movie Trivia",
    };

    private readonly Color[] segmentColors = new[]
    {
        Color.FromArgb(255, 229,  57,  53),  // vivid red
        Color.FromArgb(255, 255, 160,   0),  // warm amber
        Color.FromArgb(255,  30, 136,  30),  // rich green
        Color.FromArgb(255,  21, 101, 192),  // deep blue
        Color.FromArgb(255, 142,  36, 170),  // bold purple
    };

    private TriviaWheelViewModel? viewModel;
    private DispatcherTimer? countdownTimer;
    private DateTime nextSpinTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="TriviaWheelPage"/> class.
    /// </summary>
    public TriviaWheelPage()
    {
        this.InitializeComponent();
        this.Loaded += this.OnPageLoaded;
    }

    private static PathGeometry CreateSegmentGeometry(
        double cx,
        double cy,
        double radius,
        double startDeg,
        double endDeg)
    {
        double startRad = startDeg * Math.PI / 180.0;
        double endRad = endDeg * Math.PI / 180.0;

        Point startPoint = new Point(
            cx + (radius * Math.Cos(startRad)),
            cy + (radius * Math.Sin(startRad)));

        Point endPoint = new Point(
            cx + (radius * Math.Cos(endRad)),
            cy + (radius * Math.Sin(endRad)));

        PathFigure figure = new PathFigure
        {
            StartPoint = new Point(cx, cy),
            IsClosed = true,
        };

        figure.Segments.Add(new LineSegment { Point = startPoint });
        figure.Segments.Add(new ArcSegment
        {
            Point = endPoint,
            Size = new Windows.Foundation.Size(radius, radius),
            IsLargeArc = (endDeg - startDeg) > 180,
            SweepDirection = SweepDirection.Clockwise,
        });

        PathGeometry geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    private void StartCountdown()
    {
        this.nextSpinTime = DateTime.Today.AddDays(1);
        this.CountdownBanner.Visibility = Visibility.Visible;

        this.countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };

        this.countdownTimer.Tick += (s, e) =>
        {
            TimeSpan remaining = this.nextSpinTime - DateTime.Now;
            if (remaining <= TimeSpan.Zero)
            {
                this.countdownTimer.Stop();
                this.CountdownBanner.Visibility = Visibility.Collapsed;
                this.SpinButton.IsEnabled = true;
                this.RemainingSpinsText.Text = "1 spin available today";
            }
            else
            {
                this.CountdownText.Text = remaining.ToString(@"hh\:mm\:ss");
            }
        };

        this.countdownTimer.Start();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (App.Services.TriviaRepository is not null
            && App.Services.TriviaRewardRepository is not null
            && App.Services.SlotMachineStateRepository is not null)
        {
            this.viewModel = new TriviaWheelViewModel(
                App.Services.TriviaRepository,
                App.Services.TriviaRewardRepository,
                App.Services.SlotMachineStateRepository,
                App.CurrentUserId);

            await this.viewModel.InitializeAsync();
        }
        else
        {
            this.TriviaAvailabilityText.Text = "Trivia unavailable: no database connection.";
            this.TriviaAvailabilityText.Visibility = Visibility.Visible;
        }

        if (this.viewModel?.CanSpin == false)
        {
            if (this.viewModel.IsTriviaAvailable)
            {
                this.RemainingSpinsText.Visibility = Visibility.Collapsed;
                this.StartCountdown();
            }
            else
            {
                this.TriviaAvailabilityText.Text = this.viewModel.AvailabilityMessage;
                this.TriviaAvailabilityText.Visibility = Visibility.Visible;
            }
        }
        else
        {
            this.RemainingSpinsText.Text = this.viewModel?.RemainingSpinsText ?? "Loading...";
            this.TriviaAvailabilityText.Visibility = Visibility.Collapsed;
        }

        this.SpinButton.IsEnabled = this.viewModel is not null
            && this.viewModel.CanSpin
            && this.viewModel.IsTriviaAvailable;
        this.DrawWheel();
    }

    private void DrawWheel()
    {
        this.WheelCanvas.Children.Clear();
        double cx = 140, cy = 140, radius = 130;
        double angleStep = 360.0 / this.categories.Length;

        for (int i = 0; i < this.categories.Length; i++)
        {
            double startAngle = i * angleStep;
            double endAngle = startAngle + angleStep;

            Path path = new Path
            {
                Fill = new SolidColorBrush(this.segmentColors[i]),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 20, 20, 20)),
                StrokeThickness = 2,
                Data = CreateSegmentGeometry(cx, cy, radius, startAngle, endAngle),
            };
            this.WheelCanvas.Children.Add(path);

            double midAngleRad = (startAngle + (angleStep / 2.0)) * Math.PI / 180.0;
            double labelRadius = radius * 0.60;
            double lx = cx + (labelRadius * Math.Cos(midAngleRad)) - 44;
            double ly = cy + (labelRadius * Math.Sin(midAngleRad)) - 14;

            string shortName = this.categories[i] switch
            {
                "Oscars and Awards" => "Oscars &\nAwards",
                "General Movie Trivia" => "General\nTrivia",
                "Movie Quotes" => "Movie\nQuotes",
                _ => this.categories[i]
            };

            TextBlock label = new TextBlock
            {
                Text = shortName,
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Width = 88,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            };

            Canvas.SetLeft(label, lx);
            Canvas.SetTop(label, ly);
            this.WheelCanvas.Children.Add(label);
        }
    }

    private void SpinButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.viewModel is null || !this.viewModel.CanSpin)
        {
            return;
        }

        this.SpinButton.IsEnabled = false;

        _ = this.viewModel.RecordSpinAsync();

        Random random = new Random();
        double extraAngle = random.NextDouble() * 360.0;
        double totalRotation = (360.0 * 3) + extraAngle;

        double finalAngle = totalRotation % 360.0;
        double arrowPointsAt = (270.0 - finalAngle + 360.0) % 360.0;
        double segmentAngle = 360.0 / this.categories.Length;
        int categoryIndex = (int)(arrowPointsAt / segmentAngle) % this.categories.Length;

        DoubleAnimation animation = new DoubleAnimation
        {
            From = 0,
            To = totalRotation,
            Duration = new Duration(TimeSpan.FromSeconds(3)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        Storyboard storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, this.WheelRotation);
        Storyboard.SetTargetProperty(animation, "Angle");

        storyboard.Completed += (s, ev) =>
        {
            this.SelectedCategoryText.Text = this.categories[categoryIndex];
            this.ShowPlayingPanel();
            _ = this.LoadQuestionsAsync(this.categories[categoryIndex]);
        };

        storyboard.Begin();
    }

    private async Task LoadQuestionsAsync(string category)
    {
        if (this.viewModel is null)
        {
            return;
        }

        await this.viewModel.LoadQuestionsAsync(category);

        if (this.viewModel.NoQuestionsAvailable)
        {
            this.ShowNoQuestionsState();
            return;
        }

        this.ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (this.viewModel?.CurrentQuestion is null)
        {
            return;
        }

        TriviaQuestion question = this.viewModel.CurrentQuestion;
        this.QuestionText.Text = question.QuestionText;
        this.OptionA.Content = question.OptionA;
        this.OptionB.Content = question.OptionB;
        this.OptionC.Content = question.OptionC;
        this.OptionD.Content = question.OptionD;

        this.OptionA.IsChecked = false;
        this.OptionB.IsChecked = false;
        this.OptionC.IsChecked = false;
        this.OptionD.IsChecked = false;

        this.OptionA.Visibility = Visibility.Visible;
        this.OptionB.Visibility = Visibility.Visible;
        this.OptionC.Visibility = Visibility.Visible;
        this.OptionD.Visibility = Visibility.Visible;

        this.ProgressText.Text = this.viewModel.ProgressText;
        this.ProgressBar.Value = this.viewModel.ProgressValue;
        this.HintButton.IsEnabled = this.viewModel.IsHintAvailable;
    }

    private void HintButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.viewModel is null)
        {
            return;
        }

        this.viewModel.UseHint();
        this.HintButton.IsEnabled = false;

        IReadOnlyList<char> toHide = this.viewModel.GetHintOptionsToHide();
        foreach (char option in toHide)
        {
            switch (option)
            {
                case 'A': this.OptionA.Visibility = Visibility.Collapsed; break;
                case 'B': this.OptionB.Visibility = Visibility.Collapsed; break;
                case 'C': this.OptionC.Visibility = Visibility.Collapsed; break;
                case 'D': this.OptionD.Visibility = Visibility.Collapsed; break;
            }
        }
    }

    private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
    {
        if (this.viewModel is null)
        {
            return;
        }

        char? selected = null;
        if (this.OptionA.IsChecked == true)
        {
            selected = 'A';
        }
        else if (this.OptionB.IsChecked == true)
        {
            selected = 'B';
        }
        else if (this.OptionC.IsChecked == true)
        {
            selected = 'C';
        }
        else if (this.OptionD.IsChecked == true)
        {
            selected = 'D';
        }

        if (selected is null)
        {
            return;
        }

        this.viewModel.SubmitAnswer(selected.Value);

        if (this.viewModel.IsSessionComplete)
        {
            this.ShowResults();
        }
        else
        {
            this.ShowCurrentQuestion();
        }
    }

    private void ShowPlayingPanel()
    {
        this.IdlePanel.Visibility = Visibility.Collapsed;
        this.ResultsPanel.Visibility = Visibility.Collapsed;
        this.PlayingPanel.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Returns the page to the idle layout with a message explaining that the category has no questions.
    /// </summary>
    private void ShowNoQuestionsState()
    {
        this.IdlePanel.Visibility = Visibility.Visible;
        this.ResultsPanel.Visibility = Visibility.Collapsed;
        this.PlayingPanel.Visibility = Visibility.Collapsed;

        this.IdleTitleText.Text = "No trivia available";
        this.IdleDescriptionText.Text = this.viewModel?.EmptyStateMessage
            ?? "No trivia questions are available for this category yet.";
    }

    private void ShowResults()
    {
        this.PlayingPanel.Visibility = Visibility.Collapsed;
        this.ResultsPanel.Visibility = Visibility.Visible;

        if (this.viewModel!.HasEarnedReward)
        {
            this.ResultsTitleText.Text = "🎉 Perfect Score!";
            this.ResultsTitleText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)); // gold
            this.ResultsScoreText.Text = $"{this.viewModel.Score}";
            this.ResultsScoreText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)); // gold
            this.ResultsRewardText.Text = "A free movie ticket reward has been added to your account!";
        }
        else
        {
            this.ResultsTitleText.Text = "Session Complete";
            this.ResultsTitleText.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); // neutral
            this.ResultsScoreText.Text = $"{this.viewModel.Score}";
            this.ResultsScoreText.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); // neutral
            this.ResultsRewardText.Text = "Answer all 20 correctly next time to earn a reward.";
        }
    }
}
