using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

public interface IEventJoinService
{
    Task<JoinEventResult> JoinEventAsync(int eventId, string buttonTag);
}