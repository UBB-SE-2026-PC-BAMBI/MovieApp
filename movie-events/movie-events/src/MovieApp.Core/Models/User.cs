// <copyright file="User.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

/// <summary>
/// Represents an authenticated application user.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets the internal user identifier.
    /// </summary>
    required public int Id { get; init; }

    /// <summary>
    /// Gets the external authentication provider name.
    /// </summary>
    required public string AuthProvider { get; init; }

    /// <summary>
    /// Gets the external authentication subject identifier.
    /// </summary>
    required public string AuthSubject { get; init; }

    /// <summary>
    /// Gets the username shown in the application.
    /// </summary>
    required public string Username { get; init; }

    /// <summary>
    /// Gets the stable composite identifier used for seeded-user lookup.
    /// </summary>
    public string StableId => $"{this.AuthProvider}:{this.AuthSubject}";
}
