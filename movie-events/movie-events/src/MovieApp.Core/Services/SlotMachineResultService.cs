using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Service responsible for assembling the final results for the UI, including highlighting
/// jackpot events and calculating discounts.
/// </summary>
public sealed class SlotMachineResultService
{
    private readonly IUserMovieDiscountRepository _discountRepository;
    private const int JackpotDiscountPercentage = 70;

    public SlotMachineResultService(IUserMovieDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }

    /// <summary>
    /// Prepares the final spin result for UI display, handling jackpot events and discount application.
    /// </summary>
    /// <param name="userId">The user who initiated the spin.</param>
    /// <param name="genre">The selected genre reel.</param>
    /// <param name="actor">The selected actor reel.</param>
    /// <param name="director">The selected director reel.</param>
    /// <param name="matchingEvents">Events that match the reel combination.</param>
    /// <param name="jackpotMovie">The jackpot movie if triggered (null otherwise).</param>
    /// <returns>The complete SlotMachineResult object ready for ViewModel binding.</returns>
    public async Task<SlotMachineResult> PrepareSpinResultAsync(
        int userId,
        Genre genre,
        Actor actor,
        Director director,
        List<Event> matchingEvents,
        Movie? jackpotMovie)
    {
        var result = new SlotMachineResult
        {
            Genre = genre,
            Actor = actor,
            Director = director,
            MatchingEvents = matchingEvents,
            JackpotMovie = jackpotMovie,
            JackpotDiscountApplied = jackpotMovie is not null,
            DiscountPercentage = jackpotMovie is not null ? JackpotDiscountPercentage : 0
        };

        return result;
    }
}
