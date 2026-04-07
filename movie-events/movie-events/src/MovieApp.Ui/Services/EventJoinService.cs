// <copyright file="EventJoinService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

/// <summary>
/// Handles the process of joining events and applying related rewards.
/// </summary>
public sealed class EventJoinService : IEventJoinService
{
    /// <summary>
    /// Joins the specified event for the current user and applies any associated rewards.
    /// </summary>
    /// <param name="eventId">The identifier of the event to join.</param>
    /// <param name="buttonTag">The label associated with the join action.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the result of the join attempt.
    /// </returns>
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