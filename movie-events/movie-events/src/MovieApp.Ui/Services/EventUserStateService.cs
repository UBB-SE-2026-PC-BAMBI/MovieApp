using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.Services;

public class EventUserStateService : IEventUserStateService
{
    public async Task<int> GetDiscountForEventAsync(int eventId)
    {
        User? user = App.CurrentUserService?.CurrentUser;
        if (user is null || App.UserMovieDiscountRepository is null || App.ScreeningRepository is null)
        {
            return 0;
        }

        IEnumerable<Reward> discounts = await App.UserMovieDiscountRepository.GetDiscountsForUserAsync(user.Id);

        Dictionary<int, int> bestDiscountByMovie = discounts
            .Where(r => !r.RedemptionStatus && r.EventId.HasValue)
            .GroupBy(r => r.EventId!.Value)
            .ToDictionary(g => g.Key, g => (int)g.Max(r => r.DiscountValue));

        int bestDiscount = 0;
        foreach (KeyValuePair<int, int> kvp in bestDiscountByMovie)
        {
            IEnumerable<Screening> screenings = await App.ScreeningRepository.GetByMovieIdAsync(kvp.Key);
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

    public async Task<bool> IsEventJoinedByUserAsync(int eventId)
    {
        User? user = App.CurrentUserService?.CurrentUser;
        if (user is null || App.UserEventAttendanceRepository is null)
        {
            return false;
        }

        IEnumerable<int> ids = await App.UserEventAttendanceRepository.GetJoinedEventIdsAsync(user.Id);
        return ids.Contains(eventId);
    }
}