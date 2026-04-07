// <copyright file="IWatchlistPathProvider.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

/// <summary>
/// Provides file system paths for storing and accessing the user's watchlist.
/// </summary>
public interface IWatchlistPathProvider
{
    /// <summary>
    /// Gets the folder path where the watchlist is stored.
    /// </summary>
    /// <returns>The absolute path to the watchlist folder.</returns>
    string GetWatchlistFolderPath();
}