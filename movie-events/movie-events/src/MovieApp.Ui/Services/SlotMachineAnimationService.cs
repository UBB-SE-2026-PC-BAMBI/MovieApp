// <copyright file="SlotMachineAnimationService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System.Diagnostics;
using MovieApp.Core.Models.Movie;

/// <summary>
/// Coordinates the slot machine spin animation.
/// Each reel displays multiple values sequentially and stops independently
/// in the order: Genre (first), Actor (second), Director (last).
/// The final values represent the generated roulette combination.
/// </summary>
public sealed class SlotMachineAnimationService : ISlotMachineAnimationService
{
    private const int SpinDurationMs = 2000;
    private const int ReelStopIntervalMs = 600;
    private const int TickIntervalMs = 80;

    /// <summary>
    /// Animates the slot machine reels by cycling through values and stopping each reel at a different time.
    /// </summary>
    /// <param name="finalGenre">The final genre value for the first reel.</param>
    /// <param name="finalActor">The final actor value for the second reel.</param>
    /// <param name="finalDirector">The final director value for the third reel.</param>
    /// <param name="genres">The available genre values to display during animation.</param>
    /// <param name="actors">The available actor values to display during animation.</param>
    /// <param name="directors">The available director values to display during animation.</param>
    /// <param name="onGenreChanged">Invoked whenever the genre reel value changes.</param>
    /// <param name="onActorChanged">Invoked whenever the actor reel value changes.</param>
    /// <param name="onDirectorChanged">Invoked whenever the director reel value changes.</param>
    /// <param name="onReelStopped">
    /// Invoked when a reel stops spinning (0 = Genre, 1 = Actor, 2 = Director).
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the animation.</param>
    /// <returns>A task that represents the asynchronous animation operation.</returns>
    public async Task AnimateSpinAsync(
        Genre finalGenre,
        Actor finalActor,
        Director finalDirector,
        IReadOnlyList<Genre> genres,
        IReadOnlyList<Actor> actors,
        IReadOnlyList<Director> directors,
        Action<Genre> onGenreChanged,
        Action<Actor> onActorChanged,
        Action<Director> onDirectorChanged,
        Action<int> onReelStopped,
        CancellationToken cancellationToken = default)
    {
        if (genres.Count == 0 || actors.Count == 0 || directors.Count == 0)
        {
            return;
        }

        Random random = new Random();

        int genreStopTime = SpinDurationMs;
        int actorStopTime = SpinDurationMs + ReelStopIntervalMs;
        int directorStopTime = SpinDurationMs + (2 * ReelStopIntervalMs);

        Stopwatch stopwatch = Stopwatch.StartNew();

        bool genreStopped = false;
        bool actorStopped = false;
        bool directorStopped = false;

        while (!directorStopped && !cancellationToken.IsCancellationRequested)
        {
            long elapsed = stopwatch.ElapsedMilliseconds;

            if (!genreStopped)
            {
                if (elapsed >= genreStopTime)
                {
                    onGenreChanged(finalGenre);
                    genreStopped = true;
                    onReelStopped(0);
                }
                else
                {
                    onGenreChanged(genres[random.Next(genres.Count)]);
                }
            }

            if (!actorStopped)
            {
                if (elapsed >= actorStopTime)
                {
                    onActorChanged(finalActor);
                    actorStopped = true;
                    onReelStopped(1);
                }
                else
                {
                    onActorChanged(actors[random.Next(actors.Count)]);
                }
            }

            if (!directorStopped)
            {
                if (elapsed >= directorStopTime)
                {
                    onDirectorChanged(finalDirector);
                    directorStopped = true;
                    onReelStopped(2);
                }
                else
                {
                    onDirectorChanged(directors[random.Next(directors.Count)]);
                }
            }

            if (!directorStopped)
            {
                await Task.Delay(TickIntervalMs, cancellationToken);
            }
        }

        stopwatch.Stop();
    }
}
