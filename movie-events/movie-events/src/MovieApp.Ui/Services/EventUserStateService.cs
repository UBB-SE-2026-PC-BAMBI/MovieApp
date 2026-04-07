// <copyright file="EventUserStateService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

/// <summary>
/// Provides user-specific state information for events, such as discounts and participation status.
/// </summary>
public class EventUserStateService : IEventUserStateService
{
    /// <summary>
    /// Gets the highest available discount for the specified event based on the current user's rewards.
    /// </summary>
    /// <param name="eventId">The identifier of the event.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the maximum discount percentage.
    /// </returns>
    public async Task<int> GetDiscountForEventAsync(int eventId)
    {
        User? user = App.Services.CurrentUserService?.CurrentUser;
        if (user is null || App.Services.UserMovieDiscountRepository is null || App.Services.ScreeningRepository is null)
        {
            return 0;
        }

        IEnumerable<Reward> discounts = await App.Services.UserMovieDiscountRepository.GetDiscountsForUserAsync(user.Id);

        Dictionary<int, int> bestDiscountByMovie = discounts
            .Where(r => !r.RedemptionStatus && r.EventId.HasValue)
            .GroupBy(r => r.EventId!.Value)
            .ToDictionary(g => g.Key, g => (int)g.Max(r => r.DiscountValue));

        int bestDiscount = 0;
        foreach (KeyValuePair<int, int> kvp in bestDiscountByMovie)
        {
            IEnumerable<Screening> screenings = await App.Services.ScreeningRepository.GetByMovieIdAsync(kvp.Key);
            foreach (Screening screening in screenings)
            {
                if (screening.EventId == eventId && kvp.Value > bestDiscount)
                {
                    bestDiscount = kvp.Value;
                }
            }
        }

        return bestDiscount;
    }

    /// <summary>
    /// Determines whether the current user has joined the specified event.
    /// </summary>
    /// <param name="eventId">The identifier of the event.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see langword="true"/> if the user has joined the event; otherwise, <see langword="false"/>.
    /// </returns>
    public async Task<bool> IsEventJoinedByUserAsync(int eventId)
    {
        User? user = App.Services.CurrentUserService?.CurrentUser;
        if (user is null || App.Services.UserEventAttendanceRepository is null)
        {
            return false;
        }

        IEnumerable<int> ids = await App.Services.UserEventAttendanceRepository.GetJoinedEventIdsAsync(user.Id);
        return ids.Contains(eventId);
    }
}