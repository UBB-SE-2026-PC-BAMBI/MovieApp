using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.Ui.Services;

public interface IAppServices
{
    ICurrentUserService? CurrentUserService { get; }
    IPriceWatcherRepository? PriceWatcherRepository { get; }
    IEventRepository? EventRepository { get; }
    ITriviaRepository? TriviaRepository { get; }
    ITriviaRewardRepository? TriviaRewardRepository { get; }
    IAmbassadorRepository? AmbassadorRepository { get; }
    IReferralValidator? ReferralValidator { get; }
    IMarathonRepository? MarathonRepository { get; }
    IFavoriteEventService? FavoriteEventService { get; }
    INotificationService? NotificationService { get; }
    IMovieRepository? MovieRepository { get; }
    IUserSlotMachineStateRepository? SlotMachineStateRepository { get; }
    IUserMovieDiscountRepository? UserMovieDiscountRepository { get; }
    IScreeningRepository? ScreeningRepository { get; }
    IUserEventAttendanceRepository? UserEventAttendanceRepository { get; }
    ISlotMachineService? SlotMachineService { get; }
    ISlotMachineResultService? SlotMachineResultService { get; }
    ReelAnimationService? ReelAnimationService { get; }
    SlotMachineAnimationService? SlotMachineAnimationService { get; }
    IEventUserStateService? EventUserStateService { get; }
    IEventJoinService? EventJoinService { get; }
    IWatchlistPathProvider? WatchlistPathProvider { get; }
    IMarathonService? MarathonService { get; }
}