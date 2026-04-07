// <copyright file="HomeNavigationShortcut.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Describes a static shortcut action rendered on the home page.
/// </summary>
public sealed class HomeNavigationShortcut
{
    /// <summary>
    /// Gets the user-facing shortcut title.
    /// </summary>
    required public string Title { get; init; }

    /// <summary>
    /// Gets the explanatory text shown under the shortcut title.
    /// </summary>
    required public string Description { get; init; }

    /// <summary>
    /// Gets the route tag passed to the application route resolver.
    /// </summary>
    required public string RouteTag { get; init; }
}
