// <copyright file="ReelAnimationService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System.Diagnostics;
using MovieApp.Core.Models.Movie;

/// <summary>
/// Service responsible for managing reel animation timing and sequencing in WinUI.
/// Coordinates the timing of reel animations and provides animation state to the ViewModel.
/// </summary>
public sealed class ReelAnimationService
{
    private const int AnimationDurationMs = 2000;
    private const int ReelStopDelayMs = 200;

    /// <summary>
    /// Occurs when the reel animation has completed.
    /// </summary>
    /// <remarks>
    /// Provides the final reel values via <see cref="ReelAnimationCompletedEventArgs"/>.
    /// </remarks>
    public event EventHandler<ReelAnimationCompletedEventArgs>? AnimationCompleted;

    /// <summary>
    /// Animates all reels with staggered stop timing and raises the completion event when finished.
    /// </summary>
    /// <param name="finalGenre">The final genre displayed after animation completes.</param>
    /// <param name="finalActor">The final actor displayed after animation completes.</param>
    /// <param name="finalDirector">The final director displayed after animation completes.</param>
    /// <param name="genreSequence">The sequence of genres used during animation.</param>
    /// <param name="actorSequence">The sequence of actors used during animation.</param>
    /// <param name="directorSequence">The sequence of directors used during animation.</param>
    /// <param name="cancellationToken">A token used to cancel the animation.</param>
    /// <returns>A task that represents the asynchronous animation operation.</returns>
    public async Task AnimateReelsAsync(
        Genre finalGenre,
        Actor finalActor,
        Director finalDirector,
        List<Genre> genreSequence,
        List<Actor> actorSequence,
        List<Director> directorSequence,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Start animations for all reels simultaneously
            Task animationGenreTask = this.AnimateReelAsync(genreSequence.Cast<object>().ToList(), 0, cancellationToken);
            Task animationActorTask = this.AnimateReelAsync(actorSequence.Cast<object>().ToList(), ReelStopDelayMs, cancellationToken);
            Task animationDirectorTask = this.AnimateReelAsync(directorSequence.Cast<object>().ToList(), ReelStopDelayMs * 2, cancellationToken);

            await Task.WhenAll(animationGenreTask, animationActorTask, animationDirectorTask);

            // Notify completion with final values
            this.OnAnimationCompleted(new ReelAnimationCompletedEventArgs
            {
                FinalGenre = finalGenre,
                FinalActor = finalActor,
                FinalDirector = finalDirector,
                CompletedAt = DateTime.UtcNow,
            });
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully
        }
    }

    /// <summary>
    /// Animates a single reel by cycling through values for the animation duration.
    /// </summary>
    private async Task AnimateReelAsync(List<object> values, int delayMs, CancellationToken cancellationToken)
    {
        if (values.Count == 0)
        {
            return;
        }

        // Initial delay before this reel starts
        await Task.Delay(delayMs, cancellationToken);

        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < AnimationDurationMs && !cancellationToken.IsCancellationRequested)
        {
            // Cycle through reel values
            int index = (int)((stopwatch.ElapsedMilliseconds / 50) % values.Count);

            // Here you would update the UI binding with values[index]

            // This would be connected to the ViewModel that updates display
            await Task.Delay(50, cancellationToken);
        }

        stopwatch.Stop();
    }

    private void OnAnimationCompleted(ReelAnimationCompletedEventArgs e)
    {
        this.AnimationCompleted?.Invoke(this, e);
    }
}

/// <summary>
/// Event args for when reel animation completes.
/// </summary>
public sealed class ReelAnimationCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the final genre after the animation completes.
    /// </summary>
    required public Genre FinalGenre { get; init; }

    /// <summary>
    /// Gets the final actor after the animation completes.
    /// </summary>
    required public Actor FinalActor { get; init; }

    /// <summary>
    /// Gets the final director after the animation completes.
    /// </summary>
    required public Director FinalDirector { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the animation completed.
    /// </summary>
    required public DateTime CompletedAt { get; init; }
}
