using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using MovieApp.Ui.Views;
using System;
using System.IO;

namespace MovieApp.Ui;

public partial class App : Application
{
    private Window? _window;
    private ICurrentUserService? _currentUserService;

    public static ICurrentUserService? CurrentUserService { get; private set; }
    public static IPriceWatcherRepository? PriceWatcherRepository { get; private set; }
    public static IEventRepository? EventRepository { get; private set; }
    public static ITriviaRepository? TriviaRepository { get; private set; }
    public static ITriviaRewardRepository? TriviaRewardRepository { get; private set; }
    public static IAmbassadorRepository? AmbassadorRepository { get; private set; }
    public static IReferralValidator? ReferralValidator { get; private set; }
    public static MainWindow? CurrentMainWindow { get; private set; }
    public static IConfigurationRoot? Configuration { get; private set; }
    public static IMarathonRepository? MarathonRepository { get; private set; }
    public static IFavoriteEventService? FavoriteEventService { get; private set; }
    public static INotificationService? NotificationService { get; private set; }
    public static int CurrentUserId { get; private set; }
    public static IMovieRepository? MovieRepository { get; private set; }
    public static IUserSlotMachineStateRepository? SlotMachineStateRepository { get; private set; }
    public static IUserMovieDiscountRepository? UserMovieDiscountRepository { get; private set; }
    public static IScreeningRepository? ScreeningRepository { get; private set; }
    public static IUserEventAttendanceRepository? UserEventAttendanceRepository { get; private set; }
    public static SlotMachineService? SlotMachineService { get; private set; }
    public static SlotMachineResultService? SlotMachineResultService { get; private set; }
    public static ReelAnimationService? ReelAnimationService { get; private set; }
    public static SlotMachineAnimationService? SlotMachineAnimationService { get; private set; }
    public static IEventUserStateService? EventUserStateService { get; private set; }
    public static IEventJoinService? EventJoinService { get; private set; }
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

            SlotMachineService slotMachineService = new SlotMachineService(
                slotMachineStateRepository,
                movieRepository,
                eventRepository,
                userMovieDiscountRepository);

            SlotMachineResultService slotMachineResultService = new SlotMachineResultService(userMovieDiscountRepository);
            ReelAnimationService reelAnimationService = new ReelAnimationService();
            SlotMachineAnimationService slotMachineAnimationService = new SlotMachineAnimationService();

            CurrentUserService = _currentUserService;
            EventRepository = eventRepository;
            TriviaRepository = triviaRepository;
            TriviaRewardRepository = triviaRewardRepository;
            AmbassadorRepository = ambassadorRepository;
            ReferralValidator = new ReferralValidator(ambassadorRepository);
            FavoriteEventService = new FavoriteEventService(favoriteEventRepository, eventRepository);
            NotificationService = new NotificationService(notificationRepository, favoriteEventRepository, eventRepository);
            CurrentUserId = _currentUserService.CurrentUser.Id;
            MovieRepository = movieRepository;
            SlotMachineStateRepository = slotMachineStateRepository;
            UserMovieDiscountRepository = userMovieDiscountRepository;
            ScreeningRepository = screeningRepository;
            UserEventAttendanceRepository = userEventAttendanceRepository;
            SlotMachineService = slotMachineService;
            SlotMachineResultService = slotMachineResultService;
            ReelAnimationService = reelAnimationService;
            SlotMachineAnimationService = slotMachineAnimationService;
            MarathonRepository = marathonRepository;
            EventUserStateService = new EventUserStateService();
            EventJoinService = new EventJoinService();

            string localDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MovieApp");
            Directory.CreateDirectory(localDataFolder);
            PriceWatcherRepository = new LocalPriceWatcherRepository(localDataFolder);

            StreakSpinGrantedOnLogin = await slotMachineService.RecordLoginAndCheckStreakAsync(
                _currentUserService.CurrentUser.Id);

            viewModel = new MainViewModel(_currentUserService.CurrentUser);
        }
        catch (Exception exception)
        {
            ResetRuntimeServices();
            viewModel = MainViewModel.CreateStartupError(BuildStartupErrorMessage(exception));
        }

        CurrentMainWindow = new MainWindow(viewModel, EventRepository!);
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
        CurrentUserService = null;
        EventRepository = null;
        TriviaRepository = null;
        TriviaRewardRepository = null;
        AmbassadorRepository = null;
        ReferralValidator = null;
        Configuration = null;
        MarathonRepository = null;
        FavoriteEventService = null;
        NotificationService = null;
        PriceWatcherRepository = null;
        CurrentUserId = 0;
        MovieRepository = null;
        SlotMachineStateRepository = null;
        UserMovieDiscountRepository = null;
        ScreeningRepository = null;
        SlotMachineService = null;
        SlotMachineResultService = null;
        ReelAnimationService = null;
        SlotMachineAnimationService = null;
        EventUserStateService = null;
        EventJoinService = null;
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
}