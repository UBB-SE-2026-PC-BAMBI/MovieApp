using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class PlaceholderEventPagesTests
{
    private readonly Mock<IEventRepository> _mockEventRepo;
    private readonly Mock<INotificationService> _mockNotification;

    public PlaceholderEventPagesTests()
    {
        _mockEventRepo = new Mock<IEventRepository>();
        _mockNotification = new Mock<INotificationService>();

        if (MovieApp.Ui.App.Services is AppServices appServices)
        {
            appServices.EventRepository = _mockEventRepo.Object;
            appServices.NotificationService = _mockNotification.Object;

            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.CurrentUser).Returns(new User
            {
                Id = 1,
                AuthProvider = "Test",
                AuthSubject = "Test",
                Username = "TestUser"
            });
            appServices.CurrentUserService = mockUser.Object;
        }
    }

    [Fact]
    public async Task EventManagementViewModel_InitializesToAnEmptySafeState()
    {
        var viewModel = new EventManagementViewModel();

        await viewModel.InitializeAsync();

        Assert.Empty(viewModel.AllEvents);
        Assert.Empty(viewModel.VisibleEvents);
        Assert.True(viewModel.HasNoEvents);
        Assert.False(viewModel.ShowEventList);
    }

    [Fact]
    public async Task MyEventsViewModel_InitializesToAnEmptySafeState()
    {
        var viewModel = new MyEventsViewModel();

        await viewModel.InitializeAsync();

        Assert.Empty(viewModel.AllEvents);
        Assert.Empty(viewModel.VisibleEvents);
        Assert.True(viewModel.HasNoEvents);
        Assert.False(viewModel.ShowEventList);
    }

    [Fact]
    public void CreateEventCommand_WhenFormIsValid_CallsRepositoryAddAndRefreshesEvents()
    {
        _mockEventRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event>());

        var vm = new EventManagementViewModel
        {
            FormTitle = "Movie Night",
            FormLocation = "Main Hall",
            FormDate = DateTimeOffset.Now,
            FormTime = TimeSpan.FromHours(20),
            FormPrice = 15.0
        };

        vm.CreateEventCommand.Execute(null);

        _mockEventRepo.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void CreateEventCommand_WhenTitleIsEmpty_SetsValidationMessageAndDoesNotCallRepository()
    {
        var vm = new EventManagementViewModel
        {
            FormTitle = " ",
            FormLocation = "Main Hall",
            FormDate = DateTimeOffset.Now,
            FormPrice = 15.0
        };

        vm.CreateEventCommand.Execute(null);

        Assert.Equal("Title cannot be empty.", vm.ValidationMessage);
        _mockEventRepo.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void CreateEventCommand_WhenPriceIsNegative_SetsValidationMessageAndDoesNotCallRepository()
    {
        var vm = new EventManagementViewModel
        {
            FormTitle = "Movie Night",
            FormLocation = "Main Hall",
            FormDate = DateTimeOffset.Now,
            FormPrice = -5.0
        };

        vm.CreateEventCommand.Execute(null);

        Assert.Equal("Ticket price cannot be negative.", vm.ValidationMessage);
        _mockEventRepo.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void EditEventCommand_WhenNoEventSelected_CannotExecute()
    {
        var vm = new EventManagementViewModel
        {
            SelectedEvent = null
        };

        Assert.False(vm.EditEventCommand.CanExecute(null));
    }

    [Fact]
    public void EditEventCommand_WhenEventSelected_CallsRepositoryUpdate()
    {
        _mockEventRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event>());

        var vm = new EventManagementViewModel
        {
            SelectedEvent = new Event
            {
                Id = 1,
                MaxCapacity = 100,
                Title = "Old Title",
                EventDateTime = DateTime.Now,
                LocationReference = "Old Location",
                TicketPrice = 20.0m,
                CreatorUserId = 1
            },
            FormTitle = "Updated Title",
            FormLocation = "Updated Location",
            FormDate = DateTimeOffset.Now,
            FormTime = TimeSpan.FromHours(19),
            FormPrice = 25.0
        };

        Assert.True(vm.EditEventCommand.CanExecute(null));
        vm.EditEventCommand.Execute(null);

        _mockEventRepo.Verify(r => r.UpdateEventAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void DeleteEventCommand_WhenEventSelected_CallsRepositoryDeleteAndClearsSelection()
    {
        _mockEventRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event>());

        var vm = new EventManagementViewModel
        {
            SelectedEvent = new Event
            {
                Id = 42,
                Title = "Delete Me",
                EventDateTime = DateTime.Now,
                LocationReference = "Nowhere",
                TicketPrice = 0m,
                CreatorUserId = 1
            }
        };

        Assert.True(vm.DeleteEventCommand.CanExecute(null));
        vm.DeleteEventCommand.Execute(null);

        _mockEventRepo.Verify(r => r.DeleteAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Null(vm.SelectedEvent);
    }
}