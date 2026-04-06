using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.Services;

public sealed class EventJoinService : IEventJoinService
{
    public async Task<JoinEventResult> JoinEventAsync(int eventId, string buttonTag)
    {
        User? user = App.CurrentUserService?.CurrentUser;

        if (user is null)
        {
            return new JoinEventResult { Success = false, Message = buttonTag };
        }

        if (App.UserEventAttendanceRepository is not null)
        {
            await App.UserEventAttendanceRepository.JoinAsync(user.Id, eventId);
        }

        if (App.SlotMachineService is not null)
        {
            bool granted = await App.SlotMachineService.GrantBonusSpinForEventParticipationAsync(user.Id);
            if (granted)
            {
                return new JoinEventResult { Success = true, Message = $"{buttonTag} (+1 bonus spin)" };
            }
        }

        return new JoinEventResult { Success = true, Message = buttonTag };
    }
}