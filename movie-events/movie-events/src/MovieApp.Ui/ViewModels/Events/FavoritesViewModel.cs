// <copyright file="FavoritesViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

/// <summary>
/// View model for displaying and managing the user's favorite events.
/// </summary>
public sealed class FavoritesViewModel : EventListPageViewModel
{
    private readonly IFavoriteEventService favoriteEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesViewModel"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the favorite event service is not initialized.
    /// </exception>
    public FavoritesViewModel()
    {
        this.favoriteEventService = App.Services.FavoriteEventService ?? throw new InvalidOperationException("FavoriteEventService is not initialized.");
    }

    /// <inheritdoc/>
    public override string PageTitle => "My Favorites";

    /// <summary>
    /// Removes an event from the user's favorites and refreshes the list.
    /// </summary>
    /// <param name="eventId">The identifier of the event to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveFavoriteAsync(int eventId)
    {
        User? currentUser = App.Services.CurrentUserService?.CurrentUser;
        if (currentUser == null)
        {
            return;
        }

        await this.favoriteEventService.RemoveFavoriteAsync(currentUser.Id, eventId);
        await this.InitializeAsync(); // reload events
    }

    /// <summary>
    /// Loads the list of favorite events for the current user.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the list of favorite events.
    /// </returns>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        User? currentUser = App.Services.CurrentUserService?.CurrentUser;
        if (currentUser == null)
        {
            return new List<Event>();
        }

        IReadOnlyList<FavoriteEvent> favorites = await this.favoriteEventService
            .GetFavoritesByUserAsync(currentUser.Id);

        IEventRepository eventRepository = App.Services.EventRepository 
            ?? throw new InvalidOperationException("EventRepository is not initialized.");

        List<Event> events = new List<Event>();
        foreach (var favorite in favorites)
        {
            Event? @event = await eventRepository.FindByIdAsync(favorite.EventId);
            if (@event != null)
            {
                events.Add(@event);
            }
        }

        return events;
    }
}
