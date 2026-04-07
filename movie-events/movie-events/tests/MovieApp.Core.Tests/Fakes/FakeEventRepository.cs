using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MovieApp.Core.Tests.Fakes;

public class FakeEventRepository : IEventRepository
{
    public List<Event> Items { get; } = new();

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

    public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        Items.Add(@event);
        return Task.FromResult(@event.Id);
    }

    public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.FirstOrDefault(e => e.Id == id));
    }

    public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>(Items);
    }

    public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>(Items.Where(e => e.EventType == eventType));
    }

    public Task<bool> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(e => e.Id == @event.Id);
        if (item != null)
        {
            Items.Remove(item);
            Items.Add(@event);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(e => e.Id == eventId);
        if (item != null)
        {
            item.CurrentEnrollment = newCount;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(updatedEvent, cancellationToken);
    }

    public Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(e => e.Id == eventId);
        if (item is null) return Task.FromResult(false);
        Items.Remove(item);
        return Task.FromResult(true);
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

    [Fact]
    public async Task NotifyPriceDropAsync_WhenNewPriceEqualsOldPrice_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository();

        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        eventRepo.Items.Add(CreateValidEvent(id: 1, price: 20));
        await favoriteRepo.AddAsync(100, 1);

        await service.NotifyPriceDropAsync(1, oldPrice: 20, newPrice: 20);

        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenEventRepositoryIsNull_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();

        var service = new NotificationService(notificationRepo, favoriteRepo, null!);

        await service.NotifyPriceDropAsync(1, oldPrice: 20, newPrice: 10);

        Assert.Empty(notificationRepo.Items);
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenEventDoesNotExist_DoesNotCreateNotification()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository();

        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        await favoriteRepo.AddAsync(100, 999);

        await service.NotifyPriceDropAsync(999, oldPrice: 20, newPrice: 10);

        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }

    [Fact]
    public async Task NotifySeatsAvailableAsync_WhenNewCapacityDoesNotExceedCurrentEnrollment_DoesNotNotify()
    {
        var notificationRepo = new FakeNotificationRepository();
        var favoriteRepo = new FakeFavoriteEventRepository();
        var eventRepo = new FakeEventRepository();

        var service = new NotificationService(notificationRepo, favoriteRepo, eventRepo);

        var ev = CreateValidEvent(id: 1);
        ev.CurrentEnrollment = 50;
        ev.MaxCapacity = 50;

        eventRepo.Items.Add(ev);
        await favoriteRepo.AddAsync(100, 1);

        await service.NotifySeatsAvailableAsync(1, newCapacity: 50);

        Assert.Empty(await service.GetNotificationsByUserIdAsync(100));
    }



}
