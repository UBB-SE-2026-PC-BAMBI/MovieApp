using System.Diagnostics;
using MovieApp.Core.Models.Movie;

namespace MovieApp.Ui.Services;

/// <summary>
/// Coordinates the slot machine spin animation.
/// Each reel displays multiple values sequentially and stops independently
/// in the order: Genre (first), Actor (second), Director (last).
/// The final values represent the generated roulette combination.
/// </summary>
public sealed class SlotMachineAnimationService : ISlotMachineAnimationService
{
    private const int SPIN_DURATION_MS = 2000;
    private const int REEL_STOP_INTERVAL_MS = 600;
    private const int TICK_INTERVAL_MS = 80;

    /// <summary>
    /// Animates the slot machine reels. During the spin, each reel displays
    /// multiple values from its category sequentially. Reels stop independently
    /// in the order: Genre, Actor, Director.
    /// </summary>
    /// <param name="finalGenre">The final genre value for the first reel.</param>
    /// <param name="finalActor">The final actor value for the second reel.</param>
    /// <param name="finalDirector">The final director value for the third reel.</param>
    /// <param name="genres">Available genre values to cycle through.</param>
    /// <param name="actors">Available actor values to cycle through.</param>
    /// <param name="directors">Available director values to cycle through.</param>
    /// <param name="onGenreChanged">Callback invoked when the genre reel value changes.</param>
    /// <param name="onActorChanged">Callback invoked when the actor reel value changes.</param>
    /// <param name="onDirectorChanged">Callback invoked when the director reel value changes.</param>
    /// <param name="onReelStopped">Callback invoked when a reel stops (0 = Genre, 1 = Actor, 2 = Director).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
            return;

        Random random = new Random();

        int genreStopTime = SPIN_DURATION_MS;
        int actorStopTime = SPIN_DURATION_MS + REEL_STOP_INTERVAL_MS;
        int directorStopTime = SPIN_DURATION_MS + REEL_STOP_INTERVAL_MS * 2;

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
                await Task.Delay(TICK_INTERVAL_MS, cancellationToken);
        }

        stopwatch.Stop();
    }
}
