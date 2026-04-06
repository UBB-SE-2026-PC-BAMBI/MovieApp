using System.Threading.Tasks;

namespace MovieApp.Core.Services;

public interface IEventUserStateService
{
    Task<int> GetDiscountForEventAsync(int eventId);
    Task<bool> IsEventJoinedByUserAsync(int eventId);
}