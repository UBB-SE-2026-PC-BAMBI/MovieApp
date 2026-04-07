// <copyright file="NotificationState.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;

/// <summary>
/// Defines the possible states for a user notification.
/// </summary>
public enum NotificationState
{
    /// <summary>
    /// The notification has not been viewed by the user.
    /// </summary>
    Unread = 0,

    /// <summary>
    /// The notification has been acknowledged or viewed by the user.
    /// </summary>
    Read = 1,
}