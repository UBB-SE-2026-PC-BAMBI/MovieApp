// <copyright file="DatabaseOptions.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Infrastructure;

/// <summary>
/// Shared database configuration used by SQL-backed repositories.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Gets SQL Server connection string for the MovieApp database.
    /// </summary>
    required public string ConnectionString { get; init; }
}
