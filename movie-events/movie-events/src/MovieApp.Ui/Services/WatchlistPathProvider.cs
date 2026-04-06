using System;
using System.IO;

namespace MovieApp.Ui.Services;

public sealed class WatchlistPathProvider: IWatchlistPathProvider
{
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