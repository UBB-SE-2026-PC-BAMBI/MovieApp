// <copyright file="IReferralCodeGenerator.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

/// <summary>
/// Defines the logic for generating unique referral codes for users.
/// </summary>
public interface IReferralCodeGenerator
{
    /// <summary>
    /// Generates a permanent referral code based on user data.
    /// </summary>
    /// <param name="username">The display name of the user.</param>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>A unique referral code string.</returns>
    string Generate(string username, int userIdentifier);
}