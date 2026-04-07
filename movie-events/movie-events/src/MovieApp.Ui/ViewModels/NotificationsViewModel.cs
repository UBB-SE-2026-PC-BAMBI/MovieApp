// <copyright file="NotificationsViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

/// <summary>
/// Provides the notifications screen with the current user's persisted notifications.
/// </summary>
public sealed partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationService? notificationService;
    private readonly int currentUserId;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsViewModel"/> class.
    /// Creates the view model from the application-level notification service.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required services are not initialized.
    /// </exception>
    public NotificationsViewModel()
    {
        this.notificationService = App.Services.NotificationService;
        this.currentUserId = App.CurrentUserId;

        this.OpenEventCommand = new RelayCommand(
            this.OpenEvent,
            () => this.SelectedNotification is not null);
        this.MarkAsReadCommand = new AsyncRelayCommand(
            this.MarkAsReadAsync,
            () => this.SelectedNotification is not null && this.IsServiceAvailable);
    }

    [ObservableProperty]
    public partial Notification? SelectedNotification { get; set; }

    /// <summary>
    /// Gets the current page-level notification collection.
    /// </summary>
    public ObservableCollection<Notification> Notifications { get; } = new ();

    /// <summary>
    /// Gets a value indicating whether the notification service is available.
    /// </summary>
    public bool IsServiceAvailable => this.notificationService is not null && this.currentUserId != 0;

    /// <summary>
    /// Gets the visibility of the notifications status message.
    /// </summary>
    public Visibility StatusVisibility => this.IsServiceAvailable
        ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the status message shown when notifications cannot be loaded.
    /// </summary>
    public string StatusMessage => "Notifications are unavailable because the database connection is not ready.";

    /// <summary>
    /// Gets the command that opens the event related to the selected notification.
    /// </summary>
    public ICommand OpenEventCommand { get; }

    /// <summary>
    /// Gets the command that acknowledges and removes the selected notification.
    /// </summary>
    public ICommand MarkAsReadCommand { get; }

    /// <summary>
    /// Loads the current user's notifications from the database-backed service.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        this.Notifications.Clear();

        if (!this.IsServiceAvailable)
        {
            return;
        }

        IReadOnlyList<Notification> notifications = await this.notificationService!
            .GetNotificationsByUserIdAsync(this.currentUserId);
        foreach (Notification notification in notifications)
        {
            this.Notifications.Add(notification);
        }
    }

    /// <summary>
    /// Updates command availability when the selected notification changes.
    /// </summary>
    /// <param name="value">The newly selected notification.</param>
    partial void OnSelectedNotificationChanged(Notification? value)
    {
        ((RelayCommand)OpenEventCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Opens the event associated with the selected notification.
    /// </summary>
    private void OpenEvent()
    {
        // Navigation logic for event details would go here.
    }

    /// <summary>
    /// Removes the selected notification after it has been acknowledged.
    /// </summary>
    private async Task MarkAsReadAsync()
    {
        if (this.SelectedNotification is null || !this.IsServiceAvailable)
        {
            return;
        }

        await this.notificationService!.MarkAsReadOrRemoveAsync(this.SelectedNotification.Id);
        this.Notifications.Remove(this.SelectedNotification);
        this.SelectedNotification = null;
    }
}
