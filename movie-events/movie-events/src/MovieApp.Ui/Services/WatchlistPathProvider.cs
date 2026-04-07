// <copyright file="WatchlistPathProvider.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System;
using System.IO;

/// <summary>
/// Provides the file system path for storing the user's watchlist.
/// Ensures the target directory exists before returning the path.
/// </summary>
public sealed class WatchlistPathProvider : IWatchlistPathProvider
{
    /// <summary>
    /// Gets the folder path used to store the watchlist data.
    /// Creates the directory if it does not already exist.
    /// </summary>
    /// <returns>The absolute path to the watchlist folder.</returns>
    public string GetWatchlistFolderPath()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MovieApp");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        return folderPath;
    }
}