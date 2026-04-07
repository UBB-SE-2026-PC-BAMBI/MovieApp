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
    private Event CreateValidEvent(int id = 1, decimal price = 10)
    {
        return new Event
        {
            Id = id,
            Title = "Default Title",
            TicketPrice = price,
            EventDateTime = DateTime.Now,
            LocationReference = "Default Location",
            CreatorUserId = 1,
            MaxCapacity = 100,
            CurrentEnrollment = 0
        };
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenNewPriceEqualsOldPrice_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository();
        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        eventRepo.Items.Add(CreateValidEvent(id: 1, price: 20));
        await favoriteRepo.AddAsync(100, 1);

        // Act - Price stayed at 20
        await service.NotifyPriceDropAsync(1, oldPrice: 20, newPrice: 20);

        // Assert
        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenEventDoesNotExist_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository(); // Empty repo
        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        await favoriteRepo.AddAsync(100, 999);

        // Act
        await service.NotifyPriceDropAsync(999, oldPrice: 20, newPrice: 10);

        // Assert
        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }


    [Fact]
    public async Task NotifySeatsAvailableAsync_WhenNewCapacityDoesNotExceedCurrentEnrollment_DoesNotNotify()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository();
        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        var myEvent = CreateValidEvent(id: 1);
        myEvent.CurrentEnrollment = 50;
        myEvent.MaxCapacity = 40; // Old capacity
        eventRepo.Items.Add(myEvent);

        await favoriteRepo.AddAsync(100, 1);

        // Act - New capacity is 50, which is equal to enrollment (no actual "free" seats)
        await service.NotifySeatsAvailableAsync(1, newCapacity: 50);

        // Assert
        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenEventRepositoryIsNull_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();

        // Pass null for the event repository
        var service = new NotificationService(notificationRepo, favoriteRepo, null!);

        // Act
        await service.NotifyPriceDropAsync(1, oldPrice: 20, newPrice: 10);

        // Assert
        Assert.Empty(notificationRepo.Items);
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenNewPriceIsHigherThanOldPrice_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository();
        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        
        eventRepo.Items.Add(CreateValidEvent(id: 1, price: 10));
        await favoriteRepo.AddAsync(100, 1);

        await service.NotifyPriceDropAsync(1, oldPrice: 10, newPrice: 15);

        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }

   


}
