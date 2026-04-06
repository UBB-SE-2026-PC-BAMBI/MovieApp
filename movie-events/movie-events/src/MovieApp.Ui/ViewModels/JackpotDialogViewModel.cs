using System;
using MovieApp.Core.Models.Movie;

namespace MovieApp.Ui.ViewModels;

public class JackpotDialogViewModel
{
    public Movie Movie { get; }
    public int DiscountPercentage { get; }
    public Action? CollectAction { get; set; }

    public string FormattedDiscount => $"{DiscountPercentage}%";

    public JackpotDialogViewModel(Movie movie, int discountPercentage)
    {
        Movie = movie;
        DiscountPercentage = discountPercentage;
    }
}