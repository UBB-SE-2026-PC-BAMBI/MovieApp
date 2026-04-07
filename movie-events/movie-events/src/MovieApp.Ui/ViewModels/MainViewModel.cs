// <copyright file="MainViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using MovieApp.Core.Models;

/// <summary>
/// Represents the top-level shell state shown by <c>MainWindow</c>.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="currentUser">The user resolved from the database.</param>
    public MainViewModel(User currentUser)
    {
        this.CurrentUser = currentUser;
        this.Greeting = currentUser.Username;
        this.Description = currentUser.StableId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class
    /// with a custom greeting and description.
    /// </summary>
    /// <param name="greeting">The primary shell heading.</param>
    /// <param name="description">The secondary shell text.</param>
    private MainViewModel(string greeting, string description)
    {
        this.Greeting = greeting;
        this.Description = description;
    }

    /// <summary>
    /// Gets the application title displayed in the shell header.
    /// </summary>
    public string AppTitle => "MovieApp";

    /// <summary>
    /// Gets the current database-backed user when startup succeeds.
    /// </summary>
    public User? CurrentUser { get; }

    /// <summary>
    /// Gets the primary shell heading.
    /// </summary>
    public string Greeting { get; }

    /// <summary>
    /// Gets the secondary shell text or startup error details.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the badge text shown in the shell header.
    /// </summary>
    public string UserBadgeText => this.CurrentUser?.Username[..1].ToUpperInvariant() ?? "?";

    /// <summary>
    /// Gets the shell label describing the active user or startup state.
    /// </summary>
    public string UserLabel => this.CurrentUser?.Username ?? "Database unavailable";

    /// <summary>
    /// Gets a value indicating whether the configured user was resolved from the database.
    /// </summary>
    public bool UserFoundInDatabase => this.CurrentUser is not null;

    /// <summary>
    /// Gets the index shown in the user-status combo box.
    /// </summary>
    public int UserDatabaseStateIndex => this.UserFoundInDatabase ? 0 : 1;

    /// <summary>
    /// Creates the shell view model shown when application startup fails before
    /// any database-backed feature page can be loaded.
    /// </summary>
    /// <param name="message">The error message describing the startup failure.</param>
    /// <returns>A view model representing the startup error state.</returns>
    public static MainViewModel CreateStartupError(string message)
    {
        return new MainViewModel("Startup failed", message);
    }
}
