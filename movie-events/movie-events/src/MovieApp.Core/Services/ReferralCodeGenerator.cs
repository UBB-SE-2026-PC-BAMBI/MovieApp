// <copyright file="ReferralCodeGenerator.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Generates referral codes for ambassador accounts.
/// </summary>
public sealed class ReferralCodeGenerator : IReferralCodeGenerator
{
    /// <summary>
    /// Builds a referral code from the supplied username and user identifier.
    /// </summary>
    /// <param name="username">The display name of the user.</param>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>A generated referral code string.</returns>
    public string Generate(string username, int userIdentifier)
    {
        int currentYear = DateTime.UtcNow.Year;
        string normalizedUserToken = Regex.Replace(username.ToUpperInvariant(), "[^A-Z0-9]", string.Empty);
        StringBuilder builder = new StringBuilder(normalizedUserToken.Length + 16);
        builder.Append(normalizedUserToken);
        builder.Append(currentYear);
        builder.Append(userIdentifier);
        return builder.ToString();
    }
}
