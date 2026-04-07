using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.Tests.Fakes;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task GeneratePriceDropNotificationAsync_EventHasFavoritedUsers_CreatesNotificationsForFavoritedUsers()
    {
        FakeNotificationRepository notificationRepository = new FakeNotificationRepository();
        FakeFavoriteEventRepository favoriteRepository = new FakeFavoriteEventRepository();
        NotificationService service = new NotificationService(notificationRepository, favoriteRepository);

        await favoriteRepository.AddAsync(100, 1);
        await favoriteRepository.AddAsync(101, 1);
        await favoriteRepository.AddAsync(102, 2);

        await service.GeneratePriceDropNotificationAsync(1, "Test Event");

        Assert.Single(await service.GetNotificationsByUserAsync(100));
        Assert.Single(await service.GetNotificationsByUserAsync(101));
        Assert.Empty(await service.GetNotificationsByUserAsync(102));
    }

    [Fact]
    public async Task GenerateSeatsAvailableNotificationAsync_UnreadNotificationExists_DoesNotCreateDuplicateNotification()
    {
        FakeNotificationRepository notificationRepository = new FakeNotificationRepository();
        FakeFavoriteEventRepository favoriteRepository = new FakeFavoriteEventRepository();
        NotificationService service = new NotificationService(notificationRepository, favoriteRepository);

        await favoriteRepository.AddAsync(100, 1);

        await service.GenerateSeatsAvailableNotificationAsync(1, "Test Event");
        await service.GenerateSeatsAvailableNotificationAsync(1, "Test Event");

        Assert.Single(await service.GetNotificationsByUserAsync(100));
    }

    [Fact]
    public async Task NotifyPriceDropAsync_PriceFalls_AddsPriceDropTypeNotification()
    {
        FakeNotificationRepository notificationRepository = new FakeNotificationRepository();
        FakeFavoriteEventRepository favoriteRepository = new FakeFavoriteEventRepository();
        FakeEventRepository eventRepository = new FakeEventRepository();
        NotificationService service = new NotificationService(notificationRepository, favoriteRepository, eventRepository);

        eventRepository.Items.Add(new Event { Id = 10, Title = "Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await favoriteRepository.AddAsync(100, 10);

        await service.NotifyPriceDropAsync(10, oldPrice: 20, newPrice: 10);

        Notification notification = Assert.Single(await service.GetNotificationsByUserIdAsync(100));
        Assert.Equal("PriceDrop", notification.Type);
    }

    [Fact]
    public async Task NotifySeatsAvailableAsync_UnreadNotificationExists_DoesNotCreateDuplicateNotification()
    {
        FakeNotificationRepository notificationRepository = new FakeNotificationRepository();
        FakeFavoriteEventRepository favoriteRepository = new FakeFavoriteEventRepository();
        FakeEventRepository eventRepository = new FakeEventRepository();
        NotificationService service = new NotificationService(notificationRepository, favoriteRepository, eventRepository);

        eventRepository.Items.Add(new Event
        {
            Id = 10,
            Title = "Event",
            EventDateTime = DateTime.Now,
            LocationReference = "Loc",
            TicketPrice = 10,
            CreatorUserId = 1,
            MaxCapacity = 50,
            CurrentEnrollment = 49,
        });
        await favoriteRepository.AddAsync(100, 10);

        await service.NotifySeatsAvailableAsync(10, 50);
        await service.NotifySeatsAvailableAsync(10, 50);

        Notification notification = Assert.Single(await service.GetNotificationsByUserIdAsync(100));
        Assert.Equal("SeatsAvailable", notification.Type);
    }
}
