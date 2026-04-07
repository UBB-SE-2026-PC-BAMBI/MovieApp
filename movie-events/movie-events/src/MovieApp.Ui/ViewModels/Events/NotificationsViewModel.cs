// <copyright file="NotificationsViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

/// <summary>
/// View model responsible for loading and managing user notifications.
/// </summary>
public sealed class NotificationsViewModel : ViewModelBase
{
    private readonly INotificationService notificationService;
    private bool isLoading;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsViewModel"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the notification service is not initialized.
    /// </exception>
    public NotificationsViewModel()
    {
        this.notificationService = App.Services.NotificationService
            ?? throw new InvalidOperationException("NotificationService is not initialized.");
        this.Notifications = new ObservableCollection<Notification>();
    }

    /// <summary>
    /// Gets the collection of notifications for the current user.
    /// </summary>
    public ObservableCollection<Notification> Notifications { get; }

    /// <summary>
    /// Gets a value indicating whether notifications are currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set => this.SetProperty(ref this.isLoading, value);
    }

    /// <summary>
    /// Gets a value indicating whether there are no notifications to display.
    /// </summary>
    public bool HasNoNotifications => !this.IsLoading && this.Notifications.Count == 0;

    /// <summary>
    /// Asynchronously loads notifications for the current user.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        this.IsLoading = true;
        this.OnPropertyChanged(nameof(this.HasNoNotifications));

        try
        {
            User? currentUser = App.Services.CurrentUserService?.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            IReadOnlyList<Notification> notifications = await this.notificationService
                .GetNotificationsByUserAsync(currentUser.Id);

            this.Notifications.Clear();
            foreach (Notification notification in notifications)
            {
                this.Notifications.Add(notification);
            }

            this.OnPropertyChanged(nameof(this.HasNoNotifications));
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Removes a notification and refreshes the list.
    /// </summary>
    /// <param name="notificationId">The identifier of the notification to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveNotificationAsync(int notificationId)
    {
        await this.notificationService.RemoveNotificationAsync(notificationId);
        await this.InitializeAsync();
    }
}
