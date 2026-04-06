using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

/// <summary>
/// A local, file-based implementation of <see cref="IPriceWatcherRepository"/>.
/// Stores watched events in a local JSON file.
/// </summary>
public sealed class LocalPriceWatcherRepository : IPriceWatcherRepository
{
    private readonly string _filePath;
    private const int MAX_WATCH_LIMIT = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalPriceWatcherRepository"/> class.
    /// </summary>
    /// <param name="folderPath">The directory path where the watched_events.json file will be stored.</param>
    public LocalPriceWatcherRepository(string folderPath)
    {
        _filePath = Path.Combine(folderPath, "watched_events.json");
    }

    /// <summary>
    /// Asynchronously retrieves all watched events from the local JSON file.
    /// </summary>
    /// <returns>A list of <see cref="WatchedEvent"/>. Returns an empty list if the file does not exist or fails to deserialize.</returns>
    public async Task<List<WatchedEvent>> GetAllWatchedEventsAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<WatchedEvent>();
        }

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<WatchedEvent>>(json) ?? new List<WatchedEvent>();
        }
        catch
        {
            return new List<WatchedEvent>();
        }
    }

    /// <summary>
    /// Asynchronously adds a new event to the watch list and saves it to the file.
    /// </summary>
    /// <param name="watchedEvent">The event to watch.</param>
    /// <returns>
    /// <c>true</c> if the event was added successfully; 
    /// <c>false</c> if the event is already being watched or the maximum limit of 10 items has been reached.
    /// </returns>
    public async Task<bool> AddWatchAsync(WatchedEvent watchedEvent)
    {
        List<WatchedEvent> events = await GetAllWatchedEventsAsync();

        if (events.Any(ev => ev.EventId == watchedEvent.EventId))
        {
            return false;
        }

        if (events.Count >= MAX_WATCH_LIMIT)
        {
            return false;
        }

        events.Add(watchedEvent);
        await SaveAllAsync(events);
        return true;
    }

    /// <summary>
    /// Asynchronously removes an event from the watch list by its ID and updates the file.
    /// </summary>
    /// <param name="eventId">The ID of the event to remove.</param>
    public async Task RemoveWatchAsync(int eventId)
    {
        List<WatchedEvent> events = await GetAllWatchedEventsAsync();
        WatchedEvent? itemToRemove = events.FirstOrDefault(ev => ev.EventId == eventId);

        if (itemToRemove != null)
        {
            events.Remove(itemToRemove);
            await SaveAllAsync(events);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a specific watched event by its ID.
    /// </summary>
    /// <param name="eventId">The ID of the event to retrieve.</param>
    /// <returns>The <see cref="WatchedEvent"/> if found; otherwise, <c>null</c>.</returns>
    public async Task<WatchedEvent?> GetWatchAsync(int eventId)
    {
        List<WatchedEvent> events = await GetAllWatchedEventsAsync();
        return events.FirstOrDefault(ev => ev.EventId == eventId);
    }

    /// <summary>
    /// Asynchronously determines whether a specific event is currently being watched.
    /// </summary>
    /// <param name="eventId">The ID of the event to check.</param>
    /// <returns><c>true</c> if the event is being watched; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsWatchingAsync(int eventId)
    {
        List<WatchedEvent> events = await GetAllWatchedEventsAsync();
        return events.Any(ev => ev.EventId == eventId);
    }

    /// <summary>
    /// Serializes the list of events and writes them to the local JSON file.
    /// </summary>
    /// <param name="events">The list of events to save.</param>
    private async Task SaveAllAsync(List<WatchedEvent> events)
    {
        string json = JsonSerializer.Serialize(events);
        await File.WriteAllTextAsync(_filePath, json);
    }
}