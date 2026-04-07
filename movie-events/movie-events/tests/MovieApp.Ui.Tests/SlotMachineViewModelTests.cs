using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Services;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class SlotMachineViewModelTests
{
    private readonly Mock<ISlotMachineService> _mockService;
    private readonly Mock<ISlotMachineAnimationService> _mockAnimation;

    public SlotMachineViewModelTests()
    {
        _mockService = new Mock<ISlotMachineService>();
        _mockAnimation = new Mock<ISlotMachineAnimationService>();

        _mockService.Setup(s => s.GetUserSpinStateAsync(It.IsAny<int>()))
            .ReturnsAsync(new UserSpinData { UserId = 1, DailySpinsRemaining = 5, BonusSpins = 0 });
        _mockService.Setup(s => s.GetRandomGenreAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Genre { Id = 1, Name = "Action" });
        _mockService.Setup(s => s.GetRandomActorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Actor { Id = 1, Name = "John Doe" });
        _mockService.Setup(s => s.GetRandomDirectorAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Director { Id = 1, Name = "Jane Doe" });
        _mockService.Setup(s => s.GetGenresAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Genre>());
        _mockService.Setup(s => s.GetActorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Actor>());
        _mockService.Setup(s => s.GetDirectorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Director>());
    }

    [Fact]
    public void CreateUnavailable_DisablesSpinAndPreservesOfflineMessage()
    {
        var vm = SlotMachineViewModel.CreateUnavailable("Service is offline");

        Assert.Equal(0, vm.AvailableSpins);
        Assert.False(vm.IsSpinButtonEnabled);
        Assert.Equal("Service is offline", vm.StatusMessage);
        Assert.False(vm.SpinCommand.CanExecute(null));
    }

    [Fact]
    public async Task InitializeAsync_WhenServiceReturnsSpinData_SetsAvailableSpinsAndBonusSpins()
    {
        _mockService.Setup(s => s.GetUserSpinStateAsync(1))
            .ReturnsAsync(new UserSpinData { UserId = 1, DailySpinsRemaining = 3, BonusSpins = 2 });

        var vm = new SlotMachineViewModel(1, _mockService.Object, _mockAnimation.Object);

        await vm.InitializeAsync();

        Assert.Equal(3, vm.AvailableSpins);
        Assert.Equal(2, vm.BonusSpins);
    }

    [Fact]
    public async Task SpinCommand_WhenNoSpinsAvailable_CannotExecute()
    {
        _mockService.Setup(s => s.GetUserSpinStateAsync(1))
            .ReturnsAsync(new UserSpinData { UserId = 1, DailySpinsRemaining = 0, BonusSpins = 0 });

        var vm = new SlotMachineViewModel(1, _mockService.Object, _mockAnimation.Object);
        await vm.InitializeAsync();

        Assert.False(vm.SpinCommand.CanExecute(null));
    }

    [Fact]
    public async Task SpinCommand_WhenSpinning_DisablesButtonDuringExecution()
    {
        var tcs = new TaskCompletionSource<SlotMachineResult>();
        _mockService.Setup(s => s.SpinAsync(1)).Returns(tcs.Task);

        var vm = new SlotMachineViewModel(1, _mockService.Object, _mockAnimation.Object);
        await vm.InitializeAsync();

        Assert.True(vm.SpinCommand.CanExecute(null));

        vm.SpinCommand.Execute(null);

        Assert.True(vm.IsSpinning);
        Assert.False(vm.SpinCommand.CanExecute(null));

        tcs.SetResult(new SlotMachineResult { MatchingEvents = new List<Event>() });
    }

    [Fact]
    public async Task SpinAsync_WhenJackpotHit_RaisesJackpotHitEventWithCorrectMovie()
    {
        var jackpotMovie = new Movie { Id = 99, Title = "Golden Movie" };
        var result = new SlotMachineResult
        {
            MatchingEvents = new List<Event>(),
            JackpotEventIds = new HashSet<int>(),
            JackpotDiscountApplied = true,
            JackpotMovie = jackpotMovie,
            DiscountPercentage = 70
        };

        _mockService.Setup(s => s.SpinAsync(1)).ReturnsAsync(result);
        _mockAnimation.Setup(a => a.AnimateSpinAsync(
            It.IsAny<Genre>(), It.IsAny<Actor>(), It.IsAny<Director>(),
            It.IsAny<IReadOnlyList<Genre>>(), It.IsAny<IReadOnlyList<Actor>>(), It.IsAny<IReadOnlyList<Director>>(),
            It.IsAny<Action<Genre>>(), It.IsAny<Action<Actor>>(), It.IsAny<Action<Director>>(),
            It.IsAny<Action<int>>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var vm = new SlotMachineViewModel(1, _mockService.Object, _mockAnimation.Object);
        await vm.InitializeAsync();

        bool eventRaised = false;
        vm.JackpotHit += (movie, discount) =>
        {
            eventRaised = true;
            Assert.Equal(jackpotMovie, movie);
            Assert.Equal(70, discount);
        };

        vm.SpinCommand.Execute(null);

        Assert.True(eventRaised);
        Assert.Equal("🎉 JACKPOT! 70% discount earned on Golden Movie!", vm.StatusMessage);
    }

    [Fact]
    public async Task SpinAsync_WhenNoMatchingEvents_SetsCorrectStatusMessage()
    {
        var result = new SlotMachineResult
        {
            MatchingEvents = new List<Event>(),
            JackpotDiscountApplied = false
        };

        _mockService.Setup(s => s.SpinAsync(1)).ReturnsAsync(result);
        _mockAnimation.Setup(a => a.AnimateSpinAsync(
            It.IsAny<Genre>(), It.IsAny<Actor>(), It.IsAny<Director>(),
            It.IsAny<IReadOnlyList<Genre>>(), It.IsAny<IReadOnlyList<Actor>>(), It.IsAny<IReadOnlyList<Director>>(),
            It.IsAny<Action<Genre>>(), It.IsAny<Action<Actor>>(), It.IsAny<Action<Director>>(),
            It.IsAny<Action<int>>(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var vm = new SlotMachineViewModel(1, _mockService.Object, _mockAnimation.Object);
        await vm.InitializeAsync();

        vm.SpinCommand.Execute(null);

        Assert.Equal("No matching events this time. Try again!", vm.StatusMessage);
    }

    [Fact]
    public async Task RefreshSpinCountAsync_UpdatesAvailableSpinsFromService()
    {
        var vm = new SlotMachineViewModel(1, _mockService.Object, _mockAnimation.Object);

        _mockService.Setup(s => s.GetUserSpinStateAsync(1))
            .ReturnsAsync(new UserSpinData { UserId = 1, DailySpinsRemaining = 10, BonusSpins = 4 });

        await vm.RefreshSpinCountAsync();

        Assert.Equal(10, vm.AvailableSpins);
        Assert.Equal(4, vm.BonusSpins);
    }
}