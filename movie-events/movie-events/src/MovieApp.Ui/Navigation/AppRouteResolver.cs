// <copyright file="AppRouteResolver.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Navigation;

using System;
using MovieApp.Ui.Views;

/// <summary>
/// Centralizes the string route tags used by the shell and the page type
/// resolution required to navigate between top-level feature panels.
/// </summary>
public static class AppRouteResolver
{
    /// <summary>
    /// Route tag for the home page.
    /// </summary>
    public const string Home = "Home";

    /// <summary>
    /// Route tag for the user's events page.
    /// </summary>
    public const string MyEvents = "MyEvents";

    /// <summary>
    /// Route tag for the event management page.
    /// </summary>
    public const string EventManagement = "EventManagement";

    /// <summary>
    /// Route tag for the notifications page.
    /// </summary>
    public const string Notifications = "Notifications";

    /// <summary>
    /// Route tag for the rewards page.
    /// </summary>
    public const string Rewards = "Rewards";

    /// <summary>
    /// Route tag for the referral area page.
    /// </summary>
    public const string ReferralArea = "ReferralArea";

    /// <summary>
    /// Route tag for the slot machine page.
    /// </summary>
    public const string SlotMachine = "SlotMachine";

    /// <summary>
    /// Route tag for the trivia wheel page.
    /// </summary>
    public const string TriviaWheel = "TriviaWheel";

    /// <summary>
    /// Route tag for the marathons page.
    /// </summary>
    public const string Marathons = "Marathons";

    /// <summary>
    /// Route tag for the slots page.
    /// </summary>
    public const string Slots = "Slots";

    /// <summary>
    /// Route tag for the favorites page.
    /// </summary>
    public const string Favorites = "Favorites";

    /// <summary>
    /// Resolves a shell route tag to its corresponding page type.
    /// </summary>
    /// <param name="tag">The route tag emitted by the shell navigation UI.</param>
    /// <returns>The page type that should be loaded into the main frame.</returns>
    public static Type ResolvePageType(string? tag)
    {
        return tag switch
        {
            Home => typeof(HomePage),
            MyEvents => typeof(MyEventsPage),
            Favorites => typeof(FavoritesPage),
            EventManagement => typeof(EventManagementPage),
            Notifications => typeof(NotificationsPage),
            Rewards => typeof(RewardsPage),
            ReferralArea => typeof(ReferralAreaPage),
            SlotMachine => typeof(SlotMachinePage),
            TriviaWheel => typeof(TriviaWheelPage),
            Marathons => typeof(MarathonsPage),
            _ => typeof(HomePage),
        };
    }
}