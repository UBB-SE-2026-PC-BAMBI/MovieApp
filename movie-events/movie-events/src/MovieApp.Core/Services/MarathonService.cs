// <copyright file="MarathonService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

/// <summary>
/// Coordinates marathon enrollment, progress, and weekly assignment workflows.
/// </summary>
public sealed class MarathonService : IMarathonService
{
    /// <summary>
    /// The maximum possible score for a single movie verification quiz.
    /// </summary>
    private const int PerfectQuizScore = 3;

    private readonly IMarathonRepository marathonRepo;
    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarathonService"/> class.
    /// </summary>
    /// <param name="marathonRepo">The repository used for marathon data persistence.</param>
    /// <param name="currentUserService">The service used to access current user context.</param>
    public MarathonService(
        IMarathonRepository marathonRepo,
        ICurrentUserService currentUserService)
    {
        this.marathonRepo = marathonRepo;
        this.currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the current week's marathons for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of marathons assigned for the current week.</returns>
    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userId)
    {
        DateTime currentDateTime = DateTime.UtcNow;
        string weekIdentifier = $"{currentDateTime.Year}-W" +
            System.Globalization.ISOWeek.GetWeekOfYear(currentDateTime).ToString("D2");

        IEnumerable<Marathon> existingMarathons = await this.marathonRepo
            .GetWeeklyMarathonsForUserAsync(userId, weekIdentifier);

        List<Marathon> marathonList = existingMarathons.ToList();

        if (marathonList.Count == 0)
        {
            await this.marathonRepo.AssignWeeklyMarathonsAsync(userId, weekIdentifier, 10);
            marathonList = (await this.marathonRepo
                .GetWeeklyMarathonsForUserAsync(userId, weekIdentifier)).ToList();
        }

        return marathonList;
    }

    /// <summary>
    /// Gets the current user's progress for a marathon.
    /// </summary>
    /// <param name="marathonId">The unique identifier of the marathon.</param>
    /// <returns>The progress details if found; otherwise, null.</returns>
    public async Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId)
    {
        int currentUserId = this.currentUserService.CurrentUser.Id;
        return await this.marathonRepo.GetUserProgressAsync(currentUserId, marathonId);
    }

    /// <summary>
    /// Starts a marathon for the current user if prerequisite rules allow it.
    /// </summary>
    /// <param name="marathonId">The unique identifier of the marathon to start.</param>
    /// <returns>True if the marathon was started or already exists; otherwise, false.</returns>
    public async Task<bool> StartMarathonAsync(int marathonId)
    {
        int currentUserId = this.currentUserService.CurrentUser.Id;

        MarathonProgress? existingProgress = await this.marathonRepo.GetUserProgressAsync(currentUserId, marathonId);
        if (existingProgress != null)
        {
            return true;
        }

        IEnumerable<Marathon> activeMarathons = await this.marathonRepo.GetActiveMarathonsAsync();
        Marathon? selectedMarathon = activeMarathons.FirstOrDefault(marathon => marathon.Id == marathonId);

        if (selectedMarathon?.PrerequisiteMarathonId is int prerequisiteMarathonId)
        {
            bool isPrerequisiteComplete = await this.marathonRepo
                .IsPrerequisiteCompletedAsync(currentUserId, prerequisiteMarathonId);

            if (!isPrerequisiteComplete)
            {
                return false;
            }
        }

        return await this.marathonRepo.JoinMarathonAsync(currentUserId, marathonId);
    }

    /// <summary>
    /// Updates aggregate quiz accuracy after a rapid-fire verification round.
    /// </summary>
    /// <param name="marathonId">The unique identifier of the marathon.</param>
    /// <param name="correctAnswers">The number of correct answers achieved in the quiz.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateQuizResultAsync(int marathonId, int correctAnswers)
    {
        int currentUserId = this.currentUserService.CurrentUser.Id;
        MarathonProgress? marathonProgress = await this.marathonRepo.GetUserProgressAsync(currentUserId, marathonId);

        if (marathonProgress != null)
        {
            double newQuizScore = (correctAnswers / (double)PerfectQuizScore) * 100;
            marathonProgress.TriviaAccuracy = (marathonProgress.TriviaAccuracy + newQuizScore) / 2;
            marathonProgress.CompletedMoviesCount++;
            await this.marathonRepo.UpdateProgressAsync(marathonProgress);
        }
    }

    /// <summary>
    /// Logs a verified movie when the user answers verification questions correctly.
    /// </summary>
    /// <param name="marathonId">The unique identifier of the marathon.</param>
    /// <param name="movieId">The unique identifier of the movie to log.</param>
    /// <param name="correctAnswers">The number of correct answers provided for verification.</param>
    /// <returns>True if the movie was logged successfully; otherwise, false.</returns>
    public async Task<bool> LogMovieAsync(int marathonId, int movieId, int correctAnswers)
    {
        if (correctAnswers < PerfectQuizScore)
        {
            return false;
        }

        int currentUserId = this.currentUserService.CurrentUser.Id;
        MarathonProgress? marathonProgress = await this.marathonRepo.GetUserProgressAsync(currentUserId, marathonId);
        if (marathonProgress is null)
        {
            return false;
        }

        double newScore = (correctAnswers / (double)PerfectQuizScore) * 100;
        marathonProgress.TriviaAccuracy = marathonProgress.CompletedMoviesCount == 0
            ? newScore
            : (marathonProgress.TriviaAccuracy + newScore) / 2;

        marathonProgress.CompletedMoviesCount++;

        int totalMovies = await this.marathonRepo.GetMarathonMovieCountAsync(marathonId);
        if (marathonProgress.CompletedMoviesCount >= totalMovies && !marathonProgress.IsCompleted)
        {
            marathonProgress.FinishedAt = DateTime.UtcNow;
        }

        await this.marathonRepo.UpdateProgressAsync(marathonProgress);
        return true;
    }
}
