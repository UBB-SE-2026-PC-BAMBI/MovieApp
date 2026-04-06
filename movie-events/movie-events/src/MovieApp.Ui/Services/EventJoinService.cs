using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.Services;

public sealed class EventJoinService : IEventJoinService
{
    public async Task<JoinEventResult> JoinEventAsync(int eventId, string buttonTag)
    {
        User? user = App.Services.CurrentUserService?.CurrentUser;

        if (user is null)
        {
            return new JoinEventResult { Success = false, Message = buttonTag };
        }

        if (App.Services.UserEventAttendanceRepository is not null)
        {
            await App.Services.UserEventAttendanceRepository.JoinAsync(user.Id, eventId);
        }

        if (App.Services.SlotMachineService is not null)
        {
            bool granted = await App.Services.SlotMachineService.GrantBonusSpinForEventParticipationAsync(user.Id);
            if (granted)
            {
                return new JoinEventResult { Success = true, Message = $"{buttonTag} (+1 bonus spin)" };
            }
        }

        return new JoinEventResult { Success = true, Message = buttonTag };
    }
}