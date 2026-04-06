using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using MovieApp.Ui.Views;
using System;

namespace MovieApp.Ui;

public partial class App : Application
{
    private Window? _window;
    private ICurrentUserService? _currentUserService;
    public static IAppServices Services { get; private set; } = new AppServices();
    public static MainWindow? CurrentMainWindow { get; private set; }
    public static IConfigurationRoot? Configuration { get; private set; }
    public static int CurrentUserId { get; private set; }
    public static bool StreakSpinGrantedOnLogin { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainViewModel viewModel;
        ResetRuntimeServices();

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

            _currentUserService = new CurrentUserService(userRepository, bootstrapUserOptions);
            await _currentUserService.InitializeAsync();

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
                CurrentUserService = _currentUserService,
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
                WatchlistPathProvider = new WatchlistPathProvider()
            };

            string localDataFolder = appServices.WatchlistPathProvider.GetWatchlistFolderPath();
            appServices.PriceWatcherRepository = new LocalPriceWatcherRepository(localDataFolder);

            Services = appServices;

            CurrentUserId = _currentUserService.CurrentUser.Id;

            StreakSpinGrantedOnLogin = await slotMachineService.RecordLoginAndCheckStreakAsync(
                _currentUserService.CurrentUser.Id);

            viewModel = new MainViewModel(_currentUserService.CurrentUser);
        }
        catch (Exception exception)
        {
            ResetRuntimeServices();
            viewModel = MainViewModel.CreateStartupError(BuildStartupErrorMessage(exception));
        }

        CurrentMainWindow = new MainWindow(viewModel, Services.EventRepository!);
        _window = CurrentMainWindow;
        _window.Activate();
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }

    private void ResetRuntimeServices()
    {
        _currentUserService = null;
        Services = new AppServices();
        Configuration = null;
        CurrentUserId = 0;
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

    public static void EnsureServicesValid()
    {
        if (App.Services.CurrentUserService == null || App.Services.EventRepository == null || Configuration == null)
        {
            throw new InvalidOperationException("Critical runtime services have been reset or are unavailable. The application cannot continue.");
        }
    }
}