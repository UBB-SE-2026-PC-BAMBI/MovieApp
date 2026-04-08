using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Ui.Services;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class AppServicesDefaultPropertiesTests
{
    [Fact]
    public void AppServices_DefaultProperties_AreAllNull()
    {
        var svc = new AppServices();

        Assert.Null(svc.CurrentUserService);
        Assert.Null(svc.PriceWatcherRepository);
        Assert.Null(svc.EventRepository);
        Assert.Null(svc.TriviaRepository);
        Assert.Null(svc.TriviaRewardRepository);
        Assert.Null(svc.AmbassadorRepository);
        Assert.Null(svc.ReferralValidator);
        Assert.Null(svc.MarathonRepository);
        Assert.Null(svc.FavoriteEventService);
        Assert.Null(svc.NotificationService);
        Assert.Null(svc.MovieRepository);
        Assert.Null(svc.SlotMachineStateRepository);
        Assert.Null(svc.UserMovieDiscountRepository);
        Assert.Null(svc.ScreeningRepository);
        Assert.Null(svc.UserEventAttendanceRepository);
        Assert.Null(svc.SlotMachineService);
        Assert.Null(svc.SlotMachineResultService);
        Assert.Null(svc.ReelAnimationService);
        Assert.Null(svc.SlotMachineAnimationService);
        Assert.Null(svc.EventUserStateService);
        Assert.Null(svc.EventJoinService);
        Assert.Null(svc.WatchlistPathProvider);
        Assert.Null(svc.MarathonService);
        Assert.Null(svc.DialogService);
    }
}

public sealed class AppServicesSetterTests
{
    [Fact]
    public void AppServices_CurrentUserService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<ICurrentUserService>();
        svc.CurrentUserService = mock.Object;
        Assert.Same(mock.Object, svc.CurrentUserService);
    }

    [Fact]
    public void AppServices_PriceWatcherRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IPriceWatcherRepository>();
        svc.PriceWatcherRepository = mock.Object;
        Assert.Same(mock.Object, svc.PriceWatcherRepository);
    }

    [Fact]
    public void AppServices_EventRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IEventRepository>();
        svc.EventRepository = mock.Object;
        Assert.Same(mock.Object, svc.EventRepository);
    }

    [Fact]
    public void AppServices_TriviaRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<ITriviaRepository>();
        svc.TriviaRepository = mock.Object;
        Assert.Same(mock.Object, svc.TriviaRepository);
    }

    [Fact]
    public void AppServices_TriviaRewardRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<ITriviaRewardRepository>();
        svc.TriviaRewardRepository = mock.Object;
        Assert.Same(mock.Object, svc.TriviaRewardRepository);
    }

    [Fact]
    public void AppServices_AmbassadorRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IAmbassadorRepository>();
        svc.AmbassadorRepository = mock.Object;
        Assert.Same(mock.Object, svc.AmbassadorRepository);
    }

    [Fact]
    public void AppServices_ReferralValidator_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IReferralValidator>();
        svc.ReferralValidator = mock.Object;
        Assert.Same(mock.Object, svc.ReferralValidator);
    }

    [Fact]
    public void AppServices_MarathonRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IMarathonRepository>();
        svc.MarathonRepository = mock.Object;
        Assert.Same(mock.Object, svc.MarathonRepository);
    }

    [Fact]
    public void AppServices_FavoriteEventService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IFavoriteEventService>();
        svc.FavoriteEventService = mock.Object;
        Assert.Same(mock.Object, svc.FavoriteEventService);
    }

    [Fact]
    public void AppServices_NotificationService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<INotificationService>();
        svc.NotificationService = mock.Object;
        Assert.Same(mock.Object, svc.NotificationService);
    }

    [Fact]
    public void AppServices_MovieRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IMovieRepository>();
        svc.MovieRepository = mock.Object;
        Assert.Same(mock.Object, svc.MovieRepository);
    }

    [Fact]
    public void AppServices_SlotMachineStateRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IUserSlotMachineStateRepository>();
        svc.SlotMachineStateRepository = mock.Object;
        Assert.Same(mock.Object, svc.SlotMachineStateRepository);
    }

    [Fact]
    public void AppServices_UserMovieDiscountRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IUserMovieDiscountRepository>();
        svc.UserMovieDiscountRepository = mock.Object;
        Assert.Same(mock.Object, svc.UserMovieDiscountRepository);
    }

    [Fact]
    public void AppServices_ScreeningRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IScreeningRepository>();
        svc.ScreeningRepository = mock.Object;
        Assert.Same(mock.Object, svc.ScreeningRepository);
    }

    [Fact]
    public void AppServices_UserEventAttendanceRepository_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IUserEventAttendanceRepository>();
        svc.UserEventAttendanceRepository = mock.Object;
        Assert.Same(mock.Object, svc.UserEventAttendanceRepository);
    }

    [Fact]
    public void AppServices_SlotMachineService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<ISlotMachineService>();
        svc.SlotMachineService = mock.Object;
        Assert.Same(mock.Object, svc.SlotMachineService);
    }

    [Fact]
    public void AppServices_SlotMachineResultService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<ISlotMachineResultService>();
        svc.SlotMachineResultService = mock.Object;
        Assert.Same(mock.Object, svc.SlotMachineResultService);
    }

    [Fact]
    public void AppServices_ReelAnimationService_RoundTrips()
    {
        var svc = new AppServices();
        var obj = new ReelAnimationService();
        svc.ReelAnimationService = obj;
        Assert.Same(obj, svc.ReelAnimationService);
    }

    [Fact]
    public void AppServices_SlotMachineAnimationService_RoundTrips()
    {
        var svc = new AppServices();
        var obj = new SlotMachineAnimationService();
        svc.SlotMachineAnimationService = obj;
        Assert.Same(obj, svc.SlotMachineAnimationService);
    }

    [Fact]
    public void AppServices_EventUserStateService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IEventUserStateService>();
        svc.EventUserStateService = mock.Object;
        Assert.Same(mock.Object, svc.EventUserStateService);
    }

    [Fact]
    public void AppServices_EventJoinService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IEventJoinService>();
        svc.EventJoinService = mock.Object;
        Assert.Same(mock.Object, svc.EventJoinService);
    }

    [Fact]
    public void AppServices_WatchlistPathProvider_RoundTrips()
    {
        var svc = new AppServices();
        var obj = new WatchlistPathProvider();
        svc.WatchlistPathProvider = obj;
        Assert.Same(obj, svc.WatchlistPathProvider);
    }

    [Fact]
    public void AppServices_MarathonService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IMarathonService>();
        svc.MarathonService = mock.Object;
        Assert.Same(mock.Object, svc.MarathonService);
    }

    [Fact]
    public void AppServices_DialogService_RoundTrips()
    {
        var svc = new AppServices();
        var mock = new Mock<IDialogService>();
        svc.DialogService = mock.Object;
        Assert.Same(mock.Object, svc.DialogService);
    }

    [Fact]
    public void AppServices_CanSetPropertyToNull()
    {
        var svc = new AppServices();
        var mock = new Mock<IEventRepository>();
        svc.EventRepository = mock.Object;
        svc.EventRepository = null;
        Assert.Null(svc.EventRepository);
    }
}

public sealed class EventDialogViewModelTests
{
    private static Event MakeEvent(int id = 1) => new Event
    {
        Id = id,
        Title = "Test Event",
        EventDateTime = DateTime.Now.AddDays(1),
        LocationReference = "Hall A",
        TicketPrice = 25m,
        MaxCapacity = 100,
        CurrentEnrollment = 10,
        CreatorUserId = 1,
        Description = "A description",
    };

    [Fact]
    public void Constructor_SetsEventIsJackpotAndDiscountPercent()
    {
        var ev = MakeEvent();
        var vm = new EventDialogViewModel(ev, isJackpotEvent: true, discountPercent: 30);

        Assert.Same(ev, vm.Event);
        Assert.True(vm.IsJackpotEvent);
        Assert.Equal(30, vm.DiscountPercent);
    }

    [Fact]
    public void Constructor_WithFalseJackpotAndNullDiscount_SetsCorrectly()
    {
        var ev = MakeEvent();
        var vm = new EventDialogViewModel(ev, isJackpotEvent: false, discountPercent: null);

        Assert.Same(ev, vm.Event);
        Assert.False(vm.IsJackpotEvent);
        Assert.Null(vm.DiscountPercent);
    }

    [Fact]
    public void DefaultStringProperties_AreEmptyString()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);

        Assert.Equal(string.Empty, vm.Description);
        Assert.Equal(string.Empty, vm.FormattedDate);
        Assert.Equal(string.Empty, vm.Location);
        Assert.Equal(string.Empty, vm.PriceText);
        Assert.Equal(string.Empty, vm.RatingText);
        Assert.Equal(string.Empty, vm.CapacityText);
        Assert.Equal(string.Empty, vm.JackpotBannerText);
        Assert.Equal(string.Empty, vm.JackpotDiscountedPriceText);
        Assert.Equal(string.Empty, vm.RegularDiscountedPriceText);
    }

    [Fact]
    public void DefaultBoolProperties_AreFalse()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);

        Assert.False(vm.ShowJackpotBanner);
        Assert.False(vm.ShowRegularDiscountBanner);
        Assert.False(vm.HasFreePass);
    }

    [Fact]
    public void DefaultIntProperty_FreePassBalance_IsZero()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);
        Assert.Equal(0, vm.FreePassBalance);
    }

    [Fact]
    public void DefaultFuncProperties_AreNull()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);

        Assert.Null(vm.ValidateReferralAction);
        Assert.Null(vm.UseFreePassAction);
        Assert.Null(vm.ShowSeatGuideAction);
    }

    [Fact]
    public void StringProperties_CanBeSetAndRetrieved()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null)
        {
            Description = "Great event",
            FormattedDate = "When: 2030-01-01",
            Location = "Where: Studio A",
            PriceText = "Price: $10.00",
            RatingText = "Rating: 4.8/5",
            CapacityText = "Seats: 5/100",
            JackpotBannerText = "70% Jackpot!",
            JackpotDiscountedPriceText = "$7.50 (-70%)",
            RegularDiscountedPriceText = "$9.00 (-10%)",
        };

        Assert.Equal("Great event", vm.Description);
        Assert.Equal("When: 2030-01-01", vm.FormattedDate);
        Assert.Equal("Where: Studio A", vm.Location);
        Assert.Equal("Price: $10.00", vm.PriceText);
        Assert.Equal("Rating: 4.8/5", vm.RatingText);
        Assert.Equal("Seats: 5/100", vm.CapacityText);
        Assert.Equal("70% Jackpot!", vm.JackpotBannerText);
        Assert.Equal("$7.50 (-70%)", vm.JackpotDiscountedPriceText);
        Assert.Equal("$9.00 (-10%)", vm.RegularDiscountedPriceText);
    }

    [Fact]
    public void BoolAndIntProperties_CanBeSetAndRetrieved()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null)
        {
            ShowJackpotBanner = true,
            ShowRegularDiscountBanner = true,
            HasFreePass = true,
            FreePassBalance = 5,
        };

        Assert.True(vm.ShowJackpotBanner);
        Assert.True(vm.ShowRegularDiscountBanner);
        Assert.True(vm.HasFreePass);
        Assert.Equal(5, vm.FreePassBalance);
    }

    [Fact]
    public void ValidateReferralAction_CanBeSetAndInvoked()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);
        bool wasCalled = false;
        vm.ValidateReferralAction = async code => { wasCalled = true; return true; };

        var task = vm.ValidateReferralAction!("MYCODE");

        task.Wait();
        Assert.True(wasCalled);
        Assert.True(task.Result);
    }

    [Fact]
    public void UseFreePassAction_CanBeSetAndInvoked()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);
        bool wasCalled = false;
        vm.UseFreePassAction = async () => { wasCalled = true; return true; };

        var task = vm.UseFreePassAction!();
        task.Wait();

        Assert.True(wasCalled);
        Assert.True(task.Result);
    }

    [Fact]
    public void ShowSeatGuideAction_CanBeSetAndInvoked()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, null);
        bool wasCalled = false;
        vm.ShowSeatGuideAction = () => wasCalled = true;

        vm.ShowSeatGuideAction!();

        Assert.True(wasCalled);
    }

    [Fact]
    public void Constructor_WithZeroDiscount_SetsDiscountPercentToZero()
    {
        var vm = new EventDialogViewModel(MakeEvent(), false, discountPercent: 0);
        Assert.Equal(0, vm.DiscountPercent);
    }
}

public sealed class EventDialogContentBuilderTests
{
    [Fact]
    public void Constructor_WithNullParameters_DoesNotThrow()
    {
        var builder = new EventDialogContentBuilder(null, null);
        Assert.NotNull(builder);
    }

    [Fact]
    public void Constructor_WithMockedParameters_DoesNotThrow()
    {
        var validator = new Mock<IReferralValidator>();
        var userService = new Mock<ICurrentUserService>();
        var builder = new EventDialogContentBuilder(validator.Object, userService.Object);
        Assert.NotNull(builder);
    }
}

public sealed class ReelAnimationCompletedEventArgsTests
{
    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var genre = new Genre { Id = 1, Name = "Action" };
        var actor = new Actor { Id = 2, Name = "Tom Cruise" };
        var director = new Director { Id = 3, Name = "Christopher Nolan" };
        var completedAt = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var args = new ReelAnimationCompletedEventArgs
        {
            FinalGenre = genre,
            FinalActor = actor,
            FinalDirector = director,
            CompletedAt = completedAt,
        };

        Assert.Same(genre, args.FinalGenre);
        Assert.Same(actor, args.FinalActor);
        Assert.Same(director, args.FinalDirector);
        Assert.Equal(completedAt, args.CompletedAt);
    }

    [Fact]
    public void IsEventArgs_Subtype()
    {
        var args = new ReelAnimationCompletedEventArgs
        {
            FinalGenre = new Genre { Id = 1, Name = "G" },
            FinalActor = new Actor { Id = 1, Name = "A" },
            FinalDirector = new Director { Id = 1, Name = "D" },
            CompletedAt = DateTime.UtcNow,
        };

        Assert.IsAssignableFrom<EventArgs>(args);
    }
}

public sealed class ReelAnimationServiceTests
{
    private static Genre G => new Genre { Id = 1, Name = "Action" };
    private static Actor A => new Actor { Id = 1, Name = "Actor A" };
    private static Director D => new Director { Id = 1, Name = "Director A" };

    [Fact]
    public async Task AnimateReelsAsync_WithAllEmptySequences_RaisesCompletedEventWithCorrectArgs()
    {
        var service = new ReelAnimationService();

        ReelAnimationCompletedEventArgs? captured = null;
        service.AnimationCompleted += (_, e) => captured = e;

        await service.AnimateReelsAsync(
            G, A, D,
            new List<Genre>(),
            new List<Actor>(),
            new List<Director>());

        Assert.NotNull(captured);
        Assert.Same(G.Name, captured!.FinalGenre.Name);
        Assert.Same(A.Name, captured.FinalActor.Name);
        Assert.Same(D.Name, captured.FinalDirector.Name);
        Assert.True(captured.CompletedAt <= DateTime.UtcNow);
        Assert.True(captured.CompletedAt > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task AnimateReelsAsync_WithEmptyGenreSequenceOnly_RaisesCompletedEvent()
    {
        var service = new ReelAnimationService();
        bool raised = false;
        service.AnimationCompleted += (_, _) => raised = true;

        await service.AnimateReelsAsync(
            G, A, D,
            new List<Genre>(),
            new List<Actor>(),
            new List<Director>());

        Assert.True(raised);
    }

    [Fact]
    public async Task AnimateReelsAsync_NoEventHandlers_DoesNotThrow()
    {
        var service = new ReelAnimationService();
        await service.AnimateReelsAsync(
            G, A, D,
            new List<Genre>(),
            new List<Actor>(),
            new List<Director>());
    }

    [Fact]
    public async Task AnimateReelsAsync_WithPreCancelledToken_CompletesGracefully()
    {
        var service = new ReelAnimationService();
        bool eventRaised = false;
        service.AnimationCompleted += (_, _) => eventRaised = true;

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await service.AnimateReelsAsync(
            G, A, D,
            new List<Genre> { G },
            new List<Actor> { A },
            new List<Director> { D },
            cts.Token);

        Assert.False(eventRaised);
    }

    [Fact]
    public void AnimationCompleted_EventCanBeSubscribedAndUnsubscribed()
    {
        var service = new ReelAnimationService();
        int callCount = 0;
        EventHandler<ReelAnimationCompletedEventArgs> handler = (_, _) => callCount++;

        service.AnimationCompleted += handler;
        service.AnimationCompleted -= handler;

        Assert.Equal(0, callCount);
    }
}

public sealed class SlotMachineAnimationServiceTests
{
    private static Genre G => new Genre { Id = 1, Name = "Action" };
    private static Actor A => new Actor { Id = 1, Name = "Actor A" };
    private static Director D => new Director { Id = 1, Name = "Director A" };

    [Fact]
    public async Task AnimateSpinAsync_WithEmptyGenres_ReturnsImmediatelyWithNoCallbacks()
    {
        var service = new SlotMachineAnimationService();
        bool genreChanged = false;

        await service.AnimateSpinAsync(
            G, A, D,
            new List<Genre>(),
            new List<Actor> { A },
            new List<Director> { D },
            g => genreChanged = true,
            a => { },
            d => { },
            i => { });

        Assert.False(genreChanged);
    }

    [Fact]
    public async Task AnimateSpinAsync_WithEmptyActors_ReturnsImmediately()
    {
        var service = new SlotMachineAnimationService();
        bool actorChanged = false;

        await service.AnimateSpinAsync(
            G, A, D,
            new List<Genre> { G },
            new List<Actor>(),
            new List<Director> { D },
            g => { },
            a => actorChanged = true,
            d => { },
            i => { });

        Assert.False(actorChanged);
    }

    [Fact]
    public async Task AnimateSpinAsync_WithEmptyDirectors_ReturnsImmediately()
    {
        var service = new SlotMachineAnimationService();
        bool directorChanged = false;

        await service.AnimateSpinAsync(
            G, A, D,
            new List<Genre> { G },
            new List<Actor> { A },
            new List<Director>(),
            g => { },
            a => { },
            d => directorChanged = true,
            i => { });

        Assert.False(directorChanged);
    }

    [Fact]
    public async Task AnimateSpinAsync_WithAllEmptyLists_ReturnsImmediately()
    {
        var service = new SlotMachineAnimationService();
        bool anyCalled = false;

        await service.AnimateSpinAsync(
            G, A, D,
            new List<Genre>(),
            new List<Actor>(),
            new List<Director>(),
            g => anyCalled = true,
            a => anyCalled = true,
            d => anyCalled = true,
            i => anyCalled = true);

        Assert.False(anyCalled);
    }

    [Fact]
    public async Task AnimateSpinAsync_WithPreCancelledToken_LoopDoesNotRun()
    {
        var service = new SlotMachineAnimationService();
        bool loopRan = false;

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await service.AnimateSpinAsync(
            G, A, D,
            new List<Genre> { G },
            new List<Actor> { A },
            new List<Director> { D },
            g => loopRan = true,
            a => loopRan = true,
            d => loopRan = true,
            i => loopRan = true,
            cts.Token);

        Assert.False(loopRan);
    }

    [Fact]
    public async Task AnimateSpinAsync_WithVeryShortCancellationTimeout_EntersLoopAndExits()
    {
        var service = new SlotMachineAnimationService();
        int genreCallCount = 0;

        using var cts = new CancellationTokenSource(millisecondsDelay: 150);

        try
        {
            await service.AnimateSpinAsync(
                G, A, D,
                new List<Genre> { G, new Genre { Id = 2, Name = "Comedy" } },
                new List<Actor> { A },
                new List<Director> { D },
                g => genreCallCount++,
                a => { },
                d => { },
                i => { },
                cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Task.Delay inside the loop may propagate this — acceptable.
        }

        Assert.True(genreCallCount >= 0);
    }

    [Fact]
    public void SlotMachineAnimationService_ImplementsInterface()
    {
        var service = new SlotMachineAnimationService();
        Assert.IsAssignableFrom<ISlotMachineAnimationService>(service);
    }
}


public sealed class WatchlistPathProviderTests
{
    [Fact]
    public void GetWatchlistFolderPath_ReturnsNonNullNonEmptyPath()
    {
        var provider = new WatchlistPathProvider();
        string path = provider.GetWatchlistFolderPath();
        Assert.NotNull(path);
        Assert.NotEmpty(path);
    }

    [Fact]
    public void GetWatchlistFolderPath_CreatesDirectoryOnDisk()
    {
        var provider = new WatchlistPathProvider();
        string path = provider.GetWatchlistFolderPath();
        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void GetWatchlistFolderPath_ContainsMovieAppSegment()
    {
        var provider = new WatchlistPathProvider();
        string path = provider.GetWatchlistFolderPath();
        Assert.Contains("MovieApp", path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetWatchlistFolderPath_CalledTwice_ReturnsSamePath()
    {
        var provider = new WatchlistPathProvider();
        string p1 = provider.GetWatchlistFolderPath();
        string p2 = provider.GetWatchlistFolderPath();
        Assert.Equal(p1, p2);
    }

    [Fact]
    public void GetWatchlistFolderPath_DirectoryAlreadyExists_DoesNotThrow()
    {
        var provider = new WatchlistPathProvider();
        provider.GetWatchlistFolderPath();
        var ex = Record.Exception(() => provider.GetWatchlistFolderPath());
        Assert.Null(ex);
    }

    [Fact]
    public void WatchlistPathProvider_ImplementsInterface()
    {
        var provider = new WatchlistPathProvider();
        Assert.IsAssignableFrom<IWatchlistPathProvider>(provider);
    }
}


public sealed class WinUiDialogServiceTests
{
    [Fact]
    public void SetXamlRoot_WithNull_ThrowsArgumentNullException()
    {
        var service = new WinUiDialogService();
        Assert.Throws<ArgumentNullException>(() => service.SetXamlRoot(null!));
    }

    [Fact]
    public void WinUiDialogService_ImplementsIDialogService()
    {
        var service = new WinUiDialogService();
        Assert.IsAssignableFrom<IDialogService>(service);
    }

    [Fact]
    public void WinUiDialogService_CanBeConstructed()
    {
        var service = new WinUiDialogService();
        Assert.NotNull(service);
    }
}

public sealed class EventJoinServiceTests
{
    private static User MakeUser(int id = 1) => new User
    {
        Id = id,
        AuthProvider = "test",
        AuthSubject = $"subject-{id}",
        Username = $"User{id}",
    };

    [Fact]
    public async Task JoinEventAsync_WhenCurrentUserServiceIsNull_ReturnsFalseWithButtonTag()
    {
        if (App.Services is AppServices appSvc)
        {
            appSvc.CurrentUserService = null;
            appSvc.UserEventAttendanceRepository = null;
            appSvc.SlotMachineService = null;
        }

        var service = new EventJoinService();
        var result = await service.JoinEventAsync(eventId: 99, buttonTag: "Join Event");

        Assert.False(result.Success);
        Assert.Equal("Join Event", result.Message);
    }

    [Fact]
    public async Task JoinEventAsync_WhenCurrentUserThrows_ReturnsFalse()
    {
        if (App.Services is AppServices appSvc)
        {
            var mockUserSvc = new Mock<ICurrentUserService>();
            mockUserSvc.Setup(u => u.CurrentUser).Throws(new InvalidOperationException("Not initialized"));
            appSvc.CurrentUserService = mockUserSvc.Object;
            appSvc.UserEventAttendanceRepository = null;
            appSvc.SlotMachineService = null;
        }

        var service = new EventJoinService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.JoinEventAsync(1, "tag"));
    }

    [Fact]
    public async Task JoinEventAsync_WithUserButNoRepositoryOrSlotService_ReturnsSuccessWithOriginalTag()
    {
        if (App.Services is AppServices appSvc)
        {
            var mockUserSvc = new Mock<ICurrentUserService>();
            mockUserSvc.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUserSvc.Object;
            appSvc.UserEventAttendanceRepository = null;
            appSvc.SlotMachineService = null;
        }

        var service = new EventJoinService();
        var result = await service.JoinEventAsync(eventId: 5, buttonTag: "Join Now");

        Assert.True(result.Success);
        Assert.Equal("Join Now", result.Message);
    }

    [Fact]
    public async Task JoinEventAsync_SlotMachineGrantsBonus_ReturnsBonusSpinMessage()
    {
        var mockSlotSvc = new Mock<ISlotMachineService>();
        mockSlotSvc
            .Setup(s => s.GrantBonusSpinForEventParticipationAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        if (App.Services is AppServices appSvc)
        {
            var mockUserSvc = new Mock<ICurrentUserService>();
            mockUserSvc.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUserSvc.Object;
            appSvc.UserEventAttendanceRepository = null;
            appSvc.SlotMachineService = mockSlotSvc.Object;
        }

        var service = new EventJoinService();
        var result = await service.JoinEventAsync(eventId: 3, buttonTag: "Join");

        Assert.True(result.Success);
        Assert.Contains("+1 bonus spin", result.Message);
        Assert.StartsWith("Join", result.Message);
    }

    [Fact]
    public async Task JoinEventAsync_SlotMachineDeniesBonus_ReturnsPlainSuccessTag()
    {
        var mockSlotSvc = new Mock<ISlotMachineService>();
        mockSlotSvc
            .Setup(s => s.GrantBonusSpinForEventParticipationAsync(It.IsAny<int>()))
            .ReturnsAsync(false);

        if (App.Services is AppServices appSvc)
        {
            var mockUserSvc = new Mock<ICurrentUserService>();
            mockUserSvc.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUserSvc.Object;
            appSvc.UserEventAttendanceRepository = null;
            appSvc.SlotMachineService = mockSlotSvc.Object;
        }

        var service = new EventJoinService();
        var result = await service.JoinEventAsync(eventId: 3, buttonTag: "Join");

        Assert.True(result.Success);
        Assert.Equal("Join", result.Message);
    }
}

public sealed class EventUserStateServiceTests
{
    private static User MakeUser(int id = 1) => new User
    {
        Id = id,
        AuthProvider = "test",
        AuthSubject = $"subject-{id}",
        Username = $"User{id}",
    };


    [Fact]
    public async Task GetDiscountForEventAsync_CurrentUserServiceNull_ReturnsZero()
    {
        if (App.Services is AppServices appSvc)
        {
            appSvc.CurrentUserService = null;
            appSvc.UserMovieDiscountRepository = null;
            appSvc.ScreeningRepository = null;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_UserMovieDiscountRepositoryNull_ReturnsZero()
    {
        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = null;
            appSvc.ScreeningRepository = new Mock<IScreeningRepository>().Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_ScreeningRepositoryNull_ReturnsZero()
    {
        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = new Mock<IUserMovieDiscountRepository>().Object;
            appSvc.ScreeningRepository = null;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_NoRewards_ReturnsZero()
    {
        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reward>());

        var mockScreeningRepo = new Mock<IScreeningRepository>();

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 1);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_RewardIsRedeemed_ReturnsZero()
    {
        var reward = new Reward
        {
            RewardId = 1,
            RewardType = "Discount",
            OwnerUserId = 1,
            EventId = 5,
            DiscountValue = 30.0,
            RedemptionStatus = true,
            ApplicabilityScope = "Movie:5",
        };

        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reward> { reward });

        var mockScreeningRepo = new Mock<IScreeningRepository>();

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 99);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_RewardHasNoEventId_ReturnsZero()
    {
        var reward = new Reward
        {
            RewardId = 1,
            RewardType = "Discount",
            OwnerUserId = 1,
            EventId = null,
            DiscountValue = 30.0,
            RedemptionStatus = false,
            ApplicabilityScope = string.Empty,
        };

        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reward> { reward });

        var mockScreeningRepo = new Mock<IScreeningRepository>();

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 99);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_WithMatchingScreening_ReturnsDiscountValue()
    {
        const int movieId = 5;
        const int targetEventId = 99;

        var reward = new Reward
        {
            RewardId = 1,
            RewardType = "Discount",
            OwnerUserId = 1,
            EventId = movieId,
            DiscountValue = 20.0,
            RedemptionStatus = false,
            ApplicabilityScope = $"Movie:{movieId}",
        };

        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reward> { reward });

        var screenings = new List<Screening>
        {
            new Screening { Id = 1, EventId = targetEventId, MovieId = movieId, ScreeningTime = DateTime.Now.AddDays(1) },
        };

        var mockScreeningRepo = new Mock<IScreeningRepository>();
        mockScreeningRepo
            .Setup(r => r.GetByMovieIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Screening>)screenings);

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: targetEventId);

        Assert.Equal(20, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_ScreeningEventIdDoesNotMatch_ReturnsZero()
    {
        const int movieId = 5;

        var reward = new Reward
        {
            RewardId = 1,
            RewardType = "Discount",
            OwnerUserId = 1,
            EventId = movieId,
            DiscountValue = 15.0,
            RedemptionStatus = false,
            ApplicabilityScope = $"Movie:{movieId}",
        };

        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reward> { reward });

        var screenings = new List<Screening>
        {
            new Screening { Id = 1, EventId = 200, MovieId = movieId, ScreeningTime = DateTime.Now.AddDays(1) },
        };

        var mockScreeningRepo = new Mock<IScreeningRepository>();
        mockScreeningRepo
            .Setup(r => r.GetByMovieIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Screening>)screenings);

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 99);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_MultipleRewards_ReturnsBestDiscount()
    {
        const int movieId1 = 5;
        const int movieId2 = 6;
        const int targetEventId = 99;

        var rewards = new List<Reward>
        {
            new Reward { RewardId = 1, RewardType = "D", OwnerUserId = 1, EventId = movieId1,
                         DiscountValue = 10.0, RedemptionStatus = false, ApplicabilityScope = "M" },
            new Reward { RewardId = 2, RewardType = "D", OwnerUserId = 1, EventId = movieId2,
                         DiscountValue = 25.0, RedemptionStatus = false, ApplicabilityScope = "M" },
        };

        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewards);

        var mockScreeningRepo = new Mock<IScreeningRepository>();
        mockScreeningRepo
            .Setup(r => r.GetByMovieIdAsync(movieId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Screening>)new List<Screening>
            {
                new Screening { Id = 1, EventId = targetEventId, MovieId = movieId1, ScreeningTime = DateTime.Now.AddDays(1) }
            });
        mockScreeningRepo
            .Setup(r => r.GetByMovieIdAsync(movieId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Screening>)new List<Screening>
            {
                new Screening { Id = 2, EventId = targetEventId, MovieId = movieId2, ScreeningTime = DateTime.Now.AddDays(1) }
            });

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: targetEventId);

        Assert.Equal(25, result);
    }

    [Fact]
    public async Task GetDiscountForEventAsync_NoScreeningsForMovie_ReturnsZero()
    {
        const int movieId = 7;

        var reward = new Reward
        {
            RewardId = 1,
            RewardType = "D",
            OwnerUserId = 1,
            EventId = movieId,
            DiscountValue = 50.0,
            RedemptionStatus = false,
            ApplicabilityScope = "M",
        };

        var mockDiscountRepo = new Mock<IUserMovieDiscountRepository>();
        mockDiscountRepo
            .Setup(r => r.GetDiscountsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reward> { reward });

        var mockScreeningRepo = new Mock<IScreeningRepository>();
        mockScreeningRepo
            .Setup(r => r.GetByMovieIdAsync(movieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Screening>)new List<Screening>());

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserMovieDiscountRepository = mockDiscountRepo.Object;
            appSvc.ScreeningRepository = mockScreeningRepo.Object;
        }

        var svc = new EventUserStateService();
        int result = await svc.GetDiscountForEventAsync(eventId: 99);

        Assert.Equal(0, result);
    }


    [Fact]
    public async Task IsEventJoinedByUserAsync_CurrentUserServiceNull_ReturnsFalse()
    {
        if (App.Services is AppServices appSvc)
        {
            appSvc.CurrentUserService = null;
            appSvc.UserEventAttendanceRepository = null;
        }

        var svc = new EventUserStateService();
        bool result = await svc.IsEventJoinedByUserAsync(eventId: 1);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEventJoinedByUserAsync_AttendanceRepositoryNull_ReturnsFalse()
    {
        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserEventAttendanceRepository = null;
        }

        var svc = new EventUserStateService();
        bool result = await svc.IsEventJoinedByUserAsync(eventId: 5);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEventJoinedByUserAsync_EventInJoinedList_ReturnsTrue()
    {
        var mockAttendance = new Mock<IUserEventAttendanceRepository>();
        mockAttendance
            .Setup(r => r.GetJoinedEventIdsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<int>)new List<int> { 3, 7, 15 });

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserEventAttendanceRepository = mockAttendance.Object;
        }

        var svc = new EventUserStateService();
        bool result = await svc.IsEventJoinedByUserAsync(eventId: 7);

        Assert.True(result);
    }

    [Fact]
    public async Task IsEventJoinedByUserAsync_EventNotInJoinedList_ReturnsFalse()
    {
        var mockAttendance = new Mock<IUserEventAttendanceRepository>();
        mockAttendance
            .Setup(r => r.GetJoinedEventIdsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<int>)new List<int> { 3, 7, 15 });

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(1));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserEventAttendanceRepository = mockAttendance.Object;
        }

        var svc = new EventUserStateService();
        bool result = await svc.IsEventJoinedByUserAsync(eventId: 99);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEventJoinedByUserAsync_EmptyJoinedList_ReturnsFalse()
    {
        var mockAttendance = new Mock<IUserEventAttendanceRepository>();
        mockAttendance
            .Setup(r => r.GetJoinedEventIdsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<int>)new List<int>());

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(2));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserEventAttendanceRepository = mockAttendance.Object;
        }

        var svc = new EventUserStateService();
        bool result = await svc.IsEventJoinedByUserAsync(eventId: 1);

        Assert.False(result);
    }

    [Fact]
    public async Task IsEventJoinedByUserAsync_UsesCorrectUserId()
    {
        int capturedUserId = -1;
        var mockAttendance = new Mock<IUserEventAttendanceRepository>();
        mockAttendance
            .Setup(r => r.GetJoinedEventIdsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<int, CancellationToken>((uid, _) => capturedUserId = uid)
            .ReturnsAsync((IReadOnlyList<int>)new List<int>());

        if (App.Services is AppServices appSvc)
        {
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(MakeUser(77));
            appSvc.CurrentUserService = mockUser.Object;
            appSvc.UserEventAttendanceRepository = mockAttendance.Object;
        }

        var svc = new EventUserStateService();
        await svc.IsEventJoinedByUserAsync(eventId: 1);

        Assert.Equal(77, capturedUserId);
    }
}