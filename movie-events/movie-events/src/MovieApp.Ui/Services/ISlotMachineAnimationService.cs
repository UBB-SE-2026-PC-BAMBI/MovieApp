using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models.Movie;

namespace MovieApp.Ui.Services;

public interface ISlotMachineAnimationService
{
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