using MovieApp.Core.Models.Movie;

public class JackpotDialogViewModel
{
    public Movie Movie { get; }
    public int DiscountPercentage { get; }

    public JackpotDialogViewModel(Movie movie, int discountPercentage)
    {
        Movie = movie;
        DiscountPercentage = discountPercentage;
    }
}