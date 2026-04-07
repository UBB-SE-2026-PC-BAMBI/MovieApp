// <copyright file="IAppServices.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

/// <summary>
/// Defines access to application-wide services and repositories.
/// </summary>
public interface IAppServices
{
    /// <summary>
    /// Gets the current user service.
    /// </summary>
    ICurrentUserService? CurrentUserService { get; }

    /// <summary>
    /// Gets the price watcher repository.
    /// </summary>
    IPriceWatcherRepository? PriceWatcherRepository { get; }

    /// <summary>
    /// Gets the event repository.
    /// </summary>
    IEventRepository? EventRepository { get; }

    /// <summary>
    /// Gets the trivia repository.
    /// </summary>
    ITriviaRepository? TriviaRepository { get; }

    /// <summary>
    /// Gets the trivia reward repository.
    /// </summary>
    ITriviaRewardRepository? TriviaRewardRepository { get; }

    /// <summary>
    /// Gets the ambassador repository.
    /// </summary>
    IAmbassadorRepository? AmbassadorRepository { get; }

    /// <summary>
    /// Gets the referral validator.
    /// </summary>
    IReferralValidator? ReferralValidator { get; }

    /// <summary>
    /// Gets the marathon repository.
    /// </summary>
    IMarathonRepository? MarathonRepository { get; }

    /// <summary>
    /// Gets the favorite event service.
    /// </summary>
    IFavoriteEventService? FavoriteEventService { get; }

    /// <summary>
    /// Gets the notification service.
    /// </summary>
    INotificationService? NotificationService { get; }

    /// <summary>
    /// Gets the movie repository.
    /// </summary>
    IMovieRepository? MovieRepository { get; }

    /// <summary>
    /// Gets the slot machine state repository.
    /// </summary>
    IUserSlotMachineStateRepository? SlotMachineStateRepository { get; }

    /// <summary>
    /// Gets the user movie discount repository.
    /// </summary>
    IUserMovieDiscountRepository? UserMovieDiscountRepository { get; }

    /// <summary>
    /// Gets the screening repository.
    /// </summary>
    IScreeningRepository? ScreeningRepository { get; }

    /// <summary>
    /// Gets the user event attendance repository.
    /// </summary>
    IUserEventAttendanceRepository? UserEventAttendanceRepository { get; }

    /// <summary>
    /// Gets the slot machine service.
    /// </summary>
    ISlotMachineService? SlotMachineService { get; }

    /// <summary>
    /// Gets the slot machine result service.
    /// </summary>
    ISlotMachineResultService? SlotMachineResultService { get; }

    /// <summary>
    /// Gets the reel animation service.
    /// </summary>
    ReelAnimationService? ReelAnimationService { get; }

    /// <summary>
    /// Gets the slot machine animation service.
    /// </summary>
    SlotMachineAnimationService? SlotMachineAnimationService { get; }

    /// <summary>
    /// Gets the event user state service.
    /// </summary>
    IEventUserStateService? EventUserStateService { get; }

    /// <summary>
    /// Gets the event join service.
    /// </summary>
    IEventJoinService? EventJoinService { get; }

    /// <summary>
    /// Gets the watchlist path provider.
    /// </summary>
    IWatchlistPathProvider? WatchlistPathProvider { get; }

    /// <summary>
    /// Gets the marathon service.
    /// </summary>
    IMarathonService? MarathonService { get; }

    /// <summary>
    /// Gets the dialog service.
    /// </summary>
    IDialogService? DialogService { get; }
}