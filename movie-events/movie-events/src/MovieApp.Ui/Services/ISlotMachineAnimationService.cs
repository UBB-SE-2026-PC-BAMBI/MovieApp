// <copyright file="ISlotMachineAnimationService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models.Movie;

/// <summary>
/// Defines animation behavior for the slot machine feature.
/// </summary>
public interface ISlotMachineAnimationService
{
    /// <summary>
    /// Animates the slot machine spin and updates the UI during the process.
    /// </summary>
    /// <param name="finalGenre">The final genre result of the spin.</param>
    /// <param name="finalActor">The final actor result of the spin.</param>
    /// <param name="finalDirector">The final director result of the spin.</param>
    /// <param name="genres">The available genres for the animation.</param>
    /// <param name="actors">The available actors for the animation.</param>
    /// <param name="directors">The available directors for the animation.</param>
    /// <param name="onGenreChanged">Callback invoked when the genre reel changes.</param>
    /// <param name="onActorChanged">Callback invoked when the actor reel changes.</param>
    /// <param name="onDirectorChanged">Callback invoked when the director reel changes.</param>
    /// <param name="onReelStopped">Callback invoked when a reel stops spinning.</param>
    /// <param name="cancellationToken">A token to cancel the animation.</param>
    /// <returns>A task that represents the asynchronous animation operation.</returns>
    Task AnimateSpinAsync(
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
        CancellationToken cancellationToken = default);
}