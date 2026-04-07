// <copyright file="SlotMachineResultService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

/// <summary>
/// Assembles the final results for the UI, including jackpot calculation.
/// </summary>
public sealed class SlotMachineResultService : ISlotMachineResultService
{
    private const int JackpotDiscountPercentage = 70;
    private readonly IUserMovieDiscountRepository discountRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotMachineResultService"/> class.
    /// </summary>
    /// <param name="discountRepository">The repository for discount persistence.</param>
    public SlotMachineResultService(IUserMovieDiscountRepository discountRepository)
    {
        this.discountRepository = discountRepository;
    }

    /// <summary>
    /// Prepares the final spin result for UI display, handling jackpot events and discount application.
    /// </summary>
    /// <param name="userIdentifier">The user who initiated the spin.</param>
    /// <param name="genre">The selected genre reel.</param>
    /// <param name="actor">The selected actor reel.</param>
    /// <param name="director">The selected director reel.</param>
    /// <param name="matchingEvents">Events that match the reel combination.</param>
    /// <param name="jackpotMovie">The jackpot movie if triggered (null otherwise).</param>
    /// <returns>The complete result object ready for binding.</returns>
    public async Task<SlotMachineResult> PrepareSpinResultAsync(
        int userIdentifier,
        Genre genre,
        Actor actor,
        Director director,
        List<Event> matchingEvents,
        Movie? jackpotMovie)
    {
        SlotMachineResult result = new SlotMachineResult
        {
            Genre = genre,
            Actor = actor,
            Director = director,
            MatchingEvents = matchingEvents,
            JackpotMovie = jackpotMovie,
            JackpotDiscountApplied = jackpotMovie is not null,
            DiscountPercentage = jackpotMovie is not null ? JackpotDiscountPercentage : 0,
        };

        return await Task.FromResult(result);
    }
}
