// <copyright file="NotificationService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Creates and retrieves notifications related to favorited events.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository notificationRepository;
    private readonly IFavoriteEventRepository favoriteEventRepository;
    private readonly IEventRepository? eventRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class without event lookups.
    /// </summary>
    /// <param name="notificationRepository">The repository for notification persistence.</param>
    /// <param name="favoriteEventRepository">The repository for favorite link data.</param>
    public NotificationService(
        INotificationRepository notificationRepository,
        IFavoriteEventRepository favoriteEventRepository)
    {
        this.notificationRepository = notificationRepository;
        this.favoriteEventRepository = favoriteEventRepository;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class with event lookups enabled.
    /// </summary>
    /// <param name="notificationRepository">The repository for notification persistence.</param>
    /// <param name="favoriteEventRepository">The repository for favorite link data.</param>
    /// <param name="eventRepository">The repository for event metadata.</param>
    public NotificationService(
        INotificationRepository notificationRepository,
        IFavoriteEventRepository favoriteEventRepository,
        IEventRepository eventRepository)
        : this(notificationRepository, favoriteEventRepository)
    {
        this.eventRepository = eventRepository;
    }

    /// <inheritdoc />
    public Task GeneratePriceDropNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken cancellationToken = default)
    {
        return this.GenerateNotificationForFavoritesAsync(
            eventIdentifier,
            "PRICE_DROP",
            $"The ticket price for '{eventTitle}' has dropped!",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task GenerateSeatsAvailableNotificationAsync(int eventIdentifier, string eventTitle, CancellationToken cancellationToken = default)
    {
        return this.GenerateNotificationForFavoritesAsync(
            eventIdentifier,
            "SEATS_AVAILABLE",
            $"Seats are now available for '{eventTitle}'!",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userIdentifier, CancellationToken cancellationToken = default)
    {
        return this.notificationRepository.FindByUserAsync(userIdentifier, cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveNotificationAsync(int notificationIdentifier, CancellationToken cancellationToken = default)
    {
        return this.notificationRepository.RemoveAsync(notificationIdentifier, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userIdentifier, CancellationToken cancellationToken = default)
    {
        return this.notificationRepository.FindByUserAsync(userIdentifier, cancellationToken);
    }

    /// <inheritdoc />
    public async Task NotifyPriceDropAsync(int eventIdentifier, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default)
    {
        if (newPrice >= oldPrice || this.eventRepository is null)
        {
            return;
        }

        Event? eventDetails = await this.eventRepository.FindByIdAsync(eventIdentifier, cancellationToken);
        if (eventDetails is null)
        {
            return;
        }

        IReadOnlyList<FavoriteEvent> favorites = await this.favoriteEventRepository.FindByEventAsync(eventIdentifier, cancellationToken);
        foreach (FavoriteEvent favoriteEventLink in favorites)
        {
            IReadOnlyList<Notification> notifications = await this.notificationRepository.FindByUserAsync(favoriteEventLink.UserId, cancellationToken);
            if (notifications.Any(notification => notification.EventId == eventIdentifier
                && notification.Type == "PriceDrop" && notification.State == NotificationState.Unread))
            {
                continue;
            }

            await this.notificationRepository.AddAsync(
                new Notification
                {
                    Id = 0,
                    UserId = favoriteEventLink.UserId,
                    EventId = eventIdentifier,
                    Type = "PriceDrop",
                    Message = $"The price for '{eventDetails.Title}' has dropped to {newPrice:C}!",
                    State = NotificationState.Unread,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task NotifySeatsAvailableAsync(int eventIdentifier, int newCapacity, CancellationToken cancellationToken = default)
    {
        if (this.eventRepository is null)
        {
            return;
        }

        Event? eventDetails = await this.eventRepository.FindByIdAsync(eventIdentifier, cancellationToken);
        if (eventDetails is null || newCapacity <= eventDetails.CurrentEnrollment)
        {
            return;
        }

        IReadOnlyList<FavoriteEvent> favorites = await this.favoriteEventRepository.FindByEventAsync(eventIdentifier, cancellationToken);
        foreach (FavoriteEvent favoriteEventLink in favorites)
        {
            var notifications = await this.notificationRepository.FindByUserAsync(favoriteEventLink.UserId, cancellationToken);
            if (notifications.Any(notification => notification.EventId == eventIdentifier
                && notification.Type == "SeatsAvailable" && notification.State == NotificationState.Unread))
            {
                continue;
            }

            await this.notificationRepository.AddAsync(
                new Notification
                {
                    Id = 0,
                    UserId = favoriteEventLink.UserId,
                    EventId = eventIdentifier,
                    Type = "SeatsAvailable",
                    Message = $"Seats are now available for '{eventDetails.Title}'!",
                    State = NotificationState.Unread,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task MarkAsReadOrRemoveAsync(int notificationIdentifier, CancellationToken cancellationToken = default)
    {
        return this.notificationRepository.RemoveAsync(notificationIdentifier, cancellationToken);
    }

    private async Task GenerateNotificationForFavoritesAsync(int eventIdentifier, string type, string message, CancellationToken cancellationToken)
    {
        IReadOnlyList<int> favoritedUsers = await this.favoriteEventRepository.GetUsersByFavoriteEventAsync(eventIdentifier, cancellationToken);

        foreach (int userIdentifier in favoritedUsers)
        {
            IReadOnlyList<Notification> notifications = await this.notificationRepository.FindByUserAsync(userIdentifier, cancellationToken);
            Notification? recentNotification = notifications.FirstOrDefault(notification =>
                notification.EventId == eventIdentifier && notification.Type == type);
            if (recentNotification is not null && recentNotification.State == NotificationState.Unread)
            {
                continue;
            }

            await this.notificationRepository.AddAsync(
                new Notification
                {
                    Id = 0,
                    UserId = userIdentifier,
                    EventId = eventIdentifier,
                    Type = type,
                    Message = message,
                    State = NotificationState.Unread,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }
    }
}
