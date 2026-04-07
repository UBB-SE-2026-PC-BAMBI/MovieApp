// <copyright file="CurrentUserService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Resolves and caches the bootstrap user used by the application shell.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IUserRepository userRepository;
    private readonly BootstrapUserOptions bootstrapUserOptions;
    private User? currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="userRepository">The repository used to resolve user identities.</param>
    /// <param name="bootstrapUserOptions">The configuration options for the bootstrap user.</param>
    public CurrentUserService(IUserRepository userRepository, BootstrapUserOptions bootstrapUserOptions)
    {
        this.userRepository = userRepository;
        this.bootstrapUserOptions = bootstrapUserOptions;
    }

    /// <summary>
    /// Gets the initialized current user.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed before initialization.</exception>
    public User CurrentUser =>
        this.currentUser ?? throw new InvalidOperationException("The current user has not been initialized.");

    /// <summary>
    /// Loads the configured bootstrap user once and caches the result.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the bootstrap user cannot be found in persistence.</exception>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (this.currentUser is not null)
        {
            return;
        }

        this.currentUser = await this.userRepository.FindByAuthIdentityAsync(
            this.bootstrapUserOptions.AuthProvider,
            this.bootstrapUserOptions.AuthSubject,
            cancellationToken);

        if (this.currentUser is null)
        {
            throw new InvalidOperationException(
                $"The configured bootstrap user '{this.bootstrapUserOptions.AuthProvider}:{this.bootstrapUserOptions.AuthSubject}' could not be found.");
        }
    }
}
