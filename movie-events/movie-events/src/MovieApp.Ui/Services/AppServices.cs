using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.Ui.Services;

public class AppServices : IAppServices
{
    public ICurrentUserService? CurrentUserService { get; set; }
    public IPriceWatcherRepository? PriceWatcherRepository { get; set; }
    public IEventRepository? EventRepository { get; set; }
    public ITriviaRepository? TriviaRepository { get; set; }
    public ITriviaRewardRepository? TriviaRewardRepository { get; set; }
    public IAmbassadorRepository? AmbassadorRepository { get; set; }
    public IReferralValidator? ReferralValidator { get; set; }
    public IMarathonRepository? MarathonRepository { get; set; }
    public IFavoriteEventService? FavoriteEventService { get; set; }
    public INotificationService? NotificationService { get; set; }
    public IMovieRepository? MovieRepository { get; set; }
    public IUserSlotMachineStateRepository? SlotMachineStateRepository { get; set; }
    public IUserMovieDiscountRepository? UserMovieDiscountRepository { get; set; }
    public IScreeningRepository? ScreeningRepository { get; set; }
    public IUserEventAttendanceRepository? UserEventAttendanceRepository { get; set; }
    public ISlotMachineService? SlotMachineService { get; set; }
    public ISlotMachineResultService? SlotMachineResultService { get; set; }
    public ReelAnimationService? ReelAnimationService { get; set; }
    public SlotMachineAnimationService? SlotMachineAnimationService { get; set; }
    public IEventUserStateService? EventUserStateService { get; set; }
    public IEventJoinService? EventJoinService { get; set; }
    public IWatchlistPathProvider? WatchlistPathProvider { get; set; }
}