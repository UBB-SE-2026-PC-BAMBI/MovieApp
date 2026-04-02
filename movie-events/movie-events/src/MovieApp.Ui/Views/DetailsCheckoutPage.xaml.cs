using Microsoft.UI.Xaml.Controls;

namespace MovieApp.Ui.Views;

/// <summary>
/// Provides the event detail, seat-guide, and checkout layout that later feature work
/// can plug into without reshaping the purchase flow.
/// </summary>
public sealed partial class DetailsCheckoutPage : Page
{
    public DetailsCheckoutPage()
    {
        InitializeComponent();
    }

    private async void ValidateReferralCodeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ReferralCodeTextBox.Text)) return;

        if (App.ReferralValidator is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            bool isValid = await App.ReferralValidator.IsValidReferralAsync(ReferralCodeTextBox.Text, currentUser.Id);
            ReferralCodeTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
            ReferralCodeTextBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
        }
    }
}
