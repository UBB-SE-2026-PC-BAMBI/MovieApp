// <copyright file="MyEventsViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Infrastructure;

/// <summary>
/// Represents the user's personal event workspace.
/// </summary>
/// <remarks>
/// The page shell is in place, but the current implementation still returns an
/// empty list until a backing repository flow is wired in.
/// </remarks>
public sealed class MyEventsViewModel : EventListPageViewModel
{
    /// <summary>
    /// Gets or sets the currently selected watched event.
    /// </summary>
    private WatchedEvent? selectedWatchedEvent;

    /// <summary>
    /// Gets or sets the target price for the selected watched event.
    /// </summary>
    private double selectedTargetPrice;

    /// <inheritdoc/>
    public override string PageTitle => "My Events";

    /// <summary>
    /// Gets the collection of events watched by the user.
    /// </summary>
    public ObservableCollection<WatchedEvent> WatchedEvents { get; } = new ();

    /// <summary>
    /// Gets or sets the currently selected watched event.
    /// </summary>
    public WatchedEvent? SelectedWatchedEvent
    {
        get => this.selectedWatchedEvent;
        set
        {
            if (this.SetProperty(ref this.selectedWatchedEvent, value))
            {
                this.OnPropertyChanged(nameof(this.SelectedEventIdText));
                this.SelectedTargetPrice = value != null ? (double)value.TargetPrice : 0;
            }
        }
    }

    /// <summary>
    /// Gets the string representation of the selected event's identifier.
    /// </summary>
    public string SelectedEventIdText => this.SelectedWatchedEvent?.EventId.ToString() ?? string.Empty;

    /// <summary>
    /// Gets or sets the target price for the selected watched event.
    /// </summary>
    public double SelectedTargetPrice
    {
        get => this.selectedTargetPrice;
        set => this.SetProperty(ref this.selectedTargetPrice, value);
    }

    /// <summary>
    /// Loads the user's watchlist from local storage into the collection.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoadWatchlistAsync()
    {
        LocalPriceWatcherRepository priceWatcherRepository = new Infrastructure
            .LocalPriceWatcherRepository(this.GetWatchlistFolderPath());
        List<WatchedEvent> items = await priceWatcherRepository.GetAllWatchedEventsAsync();

        this.WatchedEvents.Clear();
        foreach (WatchedEvent item in items)
        {
            this.WatchedEvents.Add(item);
        }
    }

    /// <summary>
    /// Saves the currently selected watched event with the updated target price.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SaveSelectedWatchlistAsync()
    {
        if (this.SelectedWatchedEvent != null)
        {
            this.SelectedWatchedEvent.TargetPrice = (decimal)this.SelectedTargetPrice;
            LocalPriceWatcherRepository priceWatcherRepository = new LocalPriceWatcherRepository(this.GetWatchlistFolderPath());

            await priceWatcherRepository.RemoveWatchAsync(this.SelectedWatchedEvent.EventId);
            await priceWatcherRepository.AddWatchAsync(this.SelectedWatchedEvent);

            await this.LoadWatchlistAsync();
        }
    }

    /// <inheritdoc/>
    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        return Task.FromResult<IReadOnlyList<Event>>([]);
    }

    /// <summary>
    /// Gets the folder path used to store watchlist data.
    /// Ensures the directory exists before returning the path.
    /// </summary>
    /// <returns>The absolute path to the watchlist folder.</returns>
    private string GetWatchlistFolderPath()
    {
        string folderPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MovieApp");
        Directory.CreateDirectory(folderPath);
        return folderPath;
    }
}