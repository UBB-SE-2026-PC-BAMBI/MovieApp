// <copyright file="MarathonDisplayItem.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>
namespace MovieApp.Core.Models;

using System;

/// <summary>
/// A view-optimized model that combines marathon details with user-specific progress.
/// </summary>
public sealed class MarathonDisplayItem
{
    private const double RatingDivisor = 20.0;
    private const double MaximumRatingValue = 5.0;
    private const int LargeCapacityValue = 999999;
    private const int SystemCreatorId = 0;
    private const decimal FreeTicketPrice = 0;

    /// <summary>
    /// Gets the underlying marathon entity.
    /// </summary>
    required public Marathon Marathon { get; init; }

    /// <summary>
    /// Gets the total number of users participating in this marathon.
    /// </summary>
    public int ParticipantCount { get; init; }

    /// <summary>
    /// Gets the current user's aggregate accuracy percentage.
    /// </summary>
    public double UserAccuracy { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current user has joined this marathon.
    /// </summary>
    public bool IsJoinedByUser { get; init; }

    /// <summary>
    /// Gets the count of movies verified by the current user.
    /// </summary>
    public int UserMoviesVerified { get; init; }

    /// <summary>
    /// Gets the total number of movies included in this marathon.
    /// </summary>
    public int TotalMovies { get; init; }

    /// <summary>
    /// Gets the end date of the week this marathon belongs to.
    /// </summary>
    public DateTime WeekEnd { get; init; }

    /// <summary>
    /// Projects the marathon display data into a standard Event model for UI consistency.
    /// </summary>
    /// <returns>An <see cref="Event"/> representation of the marathon.</returns>
    public Event ToEvent()
    {
        var locationDescription = this.Marathon.PrerequisiteMarathonId.HasValue
            ? "Elite Marathon"
            : "Standard Marathon";

        var calculatedRating = this.IsJoinedByUser ? this.UserAccuracy / RatingDivisor : 0.0;
        calculatedRating = Math.Round(Math.Min(calculatedRating, MaximumRatingValue), 1);

        return new Event
        {
            Id = this.Marathon.Id,
            Title = this.Marathon.Title,
            Description = this.Marathon.Description ?? "A weekly themed movie marathon.",
            EventType = this.Marathon.Theme ?? "Marathon",
            EventDateTime = this.WeekEnd,
            LocationReference = locationDescription,
            TicketPrice = FreeTicketPrice,
            HistoricalRating = calculatedRating,
            MaxCapacity = LargeCapacityValue,
            CurrentEnrollment = this.ParticipantCount,
            CreatorUserId = SystemCreatorId,
        };
    }
}