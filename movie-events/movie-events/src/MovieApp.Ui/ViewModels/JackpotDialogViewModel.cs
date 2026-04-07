// <copyright file="JackpotDialogViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels;

using MovieApp.Core.Models.Movie;

/// <summary>
/// Holds the data displayed in the jackpot reward dialog after a winning spin.
/// </summary>
public class JackpotDialogViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JackpotDialogViewModel"/> class.
    /// </summary>
    /// <param name="movie">The movie for which a jackpot discount was earned.</param>
    /// <param name="discountPercentage">The discount percentage awarded.</param>
    public JackpotDialogViewModel(Movie movie, int discountPercentage)
    {
        this.Movie = movie;
        this.DiscountPercentage = discountPercentage;
    }

    /// <summary>
    /// Gets the jackpot movie associated with this reward.
    /// </summary>
    public Movie Movie { get; }

    /// <summary>
    /// Gets the discount percentage earned on the jackpot movie.
    /// </summary>
    public int DiscountPercentage { get; }
}
