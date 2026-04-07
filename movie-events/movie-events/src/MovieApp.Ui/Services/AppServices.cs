// <copyright file="AppServices.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

/// <summary>
/// Provides access to application-wide services and repositories.
/// </summary>
public class AppServices : IAppServices
{
    /// <inheritdoc/>
    public ICurrentUserService? CurrentUserService { get; set; }

    /// <inheritdoc/>
    public IPriceWatcherRepository? PriceWatcherRepository { get; set; }

    /// <inheritdoc/>
    public IEventRepository? EventRepository { get; set; }

    /// <inheritdoc/>
    public ITriviaRepository? TriviaRepository { get; set; }

    /// <inheritdoc/>
    public ITriviaRewardRepository? TriviaRewardRepository { get; set; }

    /// <inheritdoc/>
    public IAmbassadorRepository? AmbassadorRepository { get; set; }

    /// <inheritdoc/>
    public IReferralValidator? ReferralValidator { get; set; }

    /// <inheritdoc/>
    public IMarathonRepository? MarathonRepository { get; set; }

    /// <inheritdoc/>
    public IFavoriteEventService? FavoriteEventService { get; set; }

    /// <inheritdoc/>
    public INotificationService? NotificationService { get; set; }

    /// <inheritdoc/>
    public IMovieRepository? MovieRepository { get; set; }

    /// <inheritdoc/>
    public IUserSlotMachineStateRepository? SlotMachineStateRepository { get; set; }

    /// <inheritdoc/>
    public IUserMovieDiscountRepository? UserMovieDiscountRepository { get; set; }

    /// <inheritdoc/>
    public IScreeningRepository? ScreeningRepository { get; set; }

    /// <inheritdoc/>
    public IUserEventAttendanceRepository? UserEventAttendanceRepository { get; set; }

    /// <inheritdoc/>
    public ISlotMachineService? SlotMachineService { get; set; }

    /// <inheritdoc/>
    public ISlotMachineResultService? SlotMachineResultService { get; set; }

    /// <inheritdoc/>
    public ReelAnimationService? ReelAnimationService { get; set; }

    /// <inheritdoc/>
    public SlotMachineAnimationService? SlotMachineAnimationService { get; set; }

    /// <inheritdoc/>
    public IEventUserStateService? EventUserStateService { get; set; }

    /// <inheritdoc/>
    public IEventJoinService? EventJoinService { get; set; }

    /// <inheritdoc/>
    public IWatchlistPathProvider? WatchlistPathProvider { get; set; }

    /// <inheritdoc/>
    public IMarathonService? MarathonService { get; set; }

    /// <inheritdoc/>
    public IDialogService? DialogService { get; set; }
}