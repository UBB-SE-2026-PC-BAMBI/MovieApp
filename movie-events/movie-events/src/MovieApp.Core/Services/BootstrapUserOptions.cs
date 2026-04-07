// <copyright file="BootstrapUserOptions.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

/// <summary>
/// Configures the bootstrap identity that is resolved as the app's current user.
/// </summary>
public sealed class BootstrapUserOptions
{
    /// <summary>
    /// Gets the external authentication provider identifier.
    /// </summary>
    required public string AuthProvider { get; init; }

    /// <summary>
    /// Gets the external authentication subject identifier.
    /// </summary>
    required public string AuthSubject { get; init; }
}
