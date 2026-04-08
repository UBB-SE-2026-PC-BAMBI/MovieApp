// <copyright file="App.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui;

using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using MovieApp.Ui.Views;
using System;

/// <summary>
/// Represents the application entry point and is responsible for initializing services and the main window.
/// </summary>
public partial class App : Application
{
    private Window? window;
    private ICurrentUserService? currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the application-wide services container.
    /// </summary>
    /// <returns>The <see cref="IAppServices"/> instance.</returns>
    public static IAppServices Services { get; private set; } = new AppServices();

    /// <summary>
    /// Gets the current main window instance.
    /// </summary>
    /// <returns>The <see cref="MainWindow"/> instance, or <c>null</c> if not initialized.</returns>
    public static MainWindow? CurrentMainWindow { get; private set; }

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    /// <returns>The <see cref="IConfigurationRoot"/> instance, or <c>null</c> if not initialized.</returns>
    public static IConfigurationRoot? Configuration { get; private set; }

    /// <summary>
    /// Gets the identifier of the currently authenticated user.
    /// </summary>
    /// <returns>The current user identifier.</returns>
    public static int CurrentUserId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a streak spin was granted on login.
    /// </summary>
    /// <returns><c>true</c> if granted; otherwise, <c>false</c>.</returns>
    public static bool StreakSpinGrantedOnLogin { get; private set; }

    /// <summary>
    /// Ensures that critical application services are initialized and valid.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required services are missing or invalid.
    /// </exception>
    public static void EnsureServicesValid()
    {
        if (App.Services.CurrentUserService == null || App.Services.EventRepository == null
            || Configuration == null)
        {
            throw new InvalidOperationException(
                "Critical runtime services have been reset or are unavailable. The application cannot continue.");
        }
    }

    /// <summary>
    /// Called when the application is launched and initializes all required services and the main window.
    /// </summary>
    /// <param name="args">Launch activation arguments.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainViewModel viewModel;
        this.ResetRuntimeServices();

        try
        {
            IConfigurationRoot configuration = BuildConfiguration();
            Configuration = configuration;

            DatabaseOptions databaseOptions = new DatabaseOptions
            {
                ConnectionString = configuration["Database:ConnectionString"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Database:ConnectionString'."),
            };

            BootstrapUserOptions bootstrapUserOptions = new BootstrapUserOptions
            {
                AuthProvider = configuration["Authentication:BootstrapUser:AuthProvider"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Authentication:BootstrapUser:AuthProvider'."),
                AuthSubject = configuration["Authentication:BootstrapUser:AuthSubject"]
                    ?? throw new InvalidOperationException("Missing configuration value 'Authentication:BootstrapUser:AuthSubject'."),
            };

            SqlUserRepository userRepository = new SqlUserRepository(databaseOptions);
            SqlEventRepository eventRepository = new SqlEventRepository(databaseOptions);
            SqlTriviaRepository triviaRepository = new SqlTriviaRepository(databaseOptions);
            SqlTriviaRewardRepository triviaRewardRepository = new SqlTriviaRewardRepository(databaseOptions);
            SqlAmbassadorRepository ambassadorRepository = new SqlAmbassadorRepository(databaseOptions);
            SqlFavoriteEventRepository favoriteEventRepository = new SqlFavoriteEventRepository(databaseOptions);
            SqlNotificationRepository notificationRepository = new SqlNotificationRepository(databaseOptions);
            SqlMovieRepository movieRepository = new SqlMovieRepository(databaseOptions);
            SqlUserSlotMachineStateRepository slotMachineStateRepository = new SqlUserSlotMachineStateRepository(databaseOptions);
            SqlUserRewardRepository userMovieDiscountRepository = new SqlUserRewardRepository(databaseOptions);
            SqlScreeningRepository screeningRepository = new SqlScreeningRepository(databaseOptions);
            SqlUserEventAttendanceRepository userEventAttendanceRepository = new SqlUserEventAttendanceRepository(databaseOptions);
            SqlMarathonRepository marathonRepository = new SqlMarathonRepository(databaseOptions);

            this.currentUserService = new CurrentUserService(userRepository, bootstrapUserOptions);
            await this.currentUserService.InitializeAsync();

            ISlotMachineService slotMachineService = new SlotMachineService(
                                                            slotMachineStateRepository,
                                                            movieRepository,
                                                            eventRepository,
                                                            userMovieDiscountRepository);

            ISlotMachineResultService slotMachineResultService = new SlotMachineResultService(userMovieDiscountRepository);
            ReelAnimationService reelAnimationService = new ReelAnimationService();
            SlotMachineAnimationService slotMachineAnimationService = new SlotMachineAnimationService();

            var appServices = new AppServices
            {
                CurrentUserService = this.currentUserService,
                EventRepository = eventRepository,
                TriviaRepository = triviaRepository,
                TriviaRewardRepository = triviaRewardRepository,
                AmbassadorRepository = ambassadorRepository,
                ReferralValidator = new ReferralValidator(ambassadorRepository),
                FavoriteEventService = new FavoriteEventService(favoriteEventRepository, eventRepository),
                NotificationService = new NotificationService(notificationRepository, favoriteEventRepository, eventRepository),
                MovieRepository = movieRepository,
                SlotMachineStateRepository = slotMachineStateRepository,
                UserMovieDiscountRepository = userMovieDiscountRepository,
                ScreeningRepository = screeningRepository,
                UserEventAttendanceRepository = userEventAttendanceRepository,
                SlotMachineService = slotMachineService,
                SlotMachineResultService = slotMachineResultService,
                ReelAnimationService = reelAnimationService,
                SlotMachineAnimationService = slotMachineAnimationService,
                MarathonRepository = marathonRepository,
                EventUserStateService = new EventUserStateService(),
                EventJoinService = new EventJoinService(),
                WatchlistPathProvider = new WatchlistPathProvider(),
                MarathonService = new MarathonService(marathonRepository, this.currentUserService),
                DialogService = new WinUiDialogService(),
            };

            string localDataFolder = appServices.WatchlistPathProvider.GetWatchlistFolderPath();
            appServices.PriceWatcherRepository = new LocalPriceWatcherRepository(localDataFolder);

            Services = appServices;

            CurrentUserId = this.currentUserService.CurrentUser.Id;

            StreakSpinGrantedOnLogin = await slotMachineService.RecordLoginAndCheckStreakAsync(
                this.currentUserService.CurrentUser.Id);

            viewModel = new MainViewModel(this.currentUserService.CurrentUser);
        }
        catch (Exception exception)
        {
            this.ResetRuntimeServices();
            viewModel = MainViewModel.CreateStartupError(BuildStartupErrorMessage(exception));
        }

        CurrentMainWindow = new MainWindow(viewModel, Services.EventRepository!);
        this.window = CurrentMainWindow;
        this.window.Activate();
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }

    private static string BuildStartupErrorMessage(Exception exception)
    {
        return "The application could not connect to the configured database."
            + Environment.NewLine
            + "Verify the database exists, the schema is applied, and appsettings.json points to a reachable SQL Server instance."
            + Environment.NewLine
            + Environment.NewLine
            + exception.Message;
    }

    private void ResetRuntimeServices()
    {
        this.currentUserService = null;
        Services = new AppServices();
        Configuration = null;
        CurrentUserId = 0;
    }
}