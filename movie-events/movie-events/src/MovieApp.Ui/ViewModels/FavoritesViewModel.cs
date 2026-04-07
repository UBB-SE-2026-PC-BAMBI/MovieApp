// <copyright file="FavoritesViewModel.cs" company="MovieApp">
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
/// Provides the favorites screen with the current user's persisted favorite events.
/// </summary>
public sealed partial class FavoritesViewModel : ObservableObject
{
    private readonly IFavoriteEventService? favoriteEventService;
    private readonly int currentUserId;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesViewModel"/> class.
    /// </summary>
    public FavoritesViewModel()
    {
        this.favoriteEventService = App.Services.FavoriteEventService;
        this.currentUserId = App.CurrentUserId;

        this.OpenDetailsCommand = new RelayCommand(this.OpenDetails, () => this.SelectedFavorite is not null);
        this.RemoveFavoriteCommand = new AsyncRelayCommand(
            this.RemoveFavoriteAsync,
            () => this.SelectedFavorite is not null && this.IsServiceAvailable);
    }

    /// <summary>
    /// Gets or sets the currently selected favorite event.
    /// </summary>
    [ObservableProperty]
    public partial Event? SelectedFavorite { get; set; }

    /// <summary>
    /// Gets the current page-level favorite collection.
    /// </summary>
    public ObservableCollection<Event> Favorites { get; } = new ();

    /// <summary>
    /// Gets a value indicating whether the favorites service is available.
    /// </summary>
    public bool IsServiceAvailable => this.favoriteEventService is not null && this.currentUserId != 0;

    /// <summary>
    /// Gets the visibility of the favorites status message.
    /// </summary>
    public Visibility StatusVisibility => this.IsServiceAvailable
        ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the status message shown when favorites cannot be loaded.
    /// </summary>
    public string StatusMessage =>
        "Favorites are unavailable because the database connection is not ready.";

    /// <summary>
    /// Gets the command that opens the selected favorite detail workflow.
    /// </summary>
    public ICommand OpenDetailsCommand { get; }

    /// <summary>
    /// Gets the command that removes the selected favorite.
    /// </summary>
    public ICommand RemoveFavoriteCommand { get; }

    /// <summary>
    /// Loads the current user's favorite events from the database-backed service.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        this.Favorites.Clear();

        if (!this.IsServiceAvailable)
        {
            return;
        }

        IReadOnlyList<Event> events = await this.favoriteEventService!.GetFavoriteEventsByUserIdAsync(this.currentUserId);
        foreach (Event favoriteEvent in events)
        {
            this.Favorites.Add(favoriteEvent);
        }
    }

    /// <summary>
    /// Opens the details view for the selected favorite event.
    /// </summary>
    private void OpenDetails()
    {
        // Navigation to details logic would go here.
    }

    /// <summary>
    /// Updates command availability when the selected favorite changes.
    /// </summary>
    /// <param name="value">The newly selected favorite event.</param>
    partial void OnSelectedFavoriteChanged(Event? value)
    {
        ((RelayCommand)OpenDetailsCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)RemoveFavoriteCommand).NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Removes the selected favorite from the persisted favorites store.
    /// </summary>
    private async Task RemoveFavoriteAsync()
    {
        if (this.SelectedFavorite is null || !this.IsServiceAvailable)
        {
            return;
        }

        await this.favoriteEventService!.RemoveFavoriteAsync(this.currentUserId, this.SelectedFavorite.Id);
        this.Favorites.Remove(this.SelectedFavorite);
        this.SelectedFavorite = null;
    }
}
