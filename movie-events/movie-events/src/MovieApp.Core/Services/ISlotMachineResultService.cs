using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;

namespace MovieApp.Core.Services;

public interface ISlotMachineResultService
{
    Task<SlotMachineResult> PrepareSpinResultAsync(
        int userId,
        Genre genre,
        Actor actor,
        Director director,
        List<Event> matchingEvents,
        Movie? jackpotMovie);
}