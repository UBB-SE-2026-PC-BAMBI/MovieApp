// <copyright file="DetailsCheckoutPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;


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

        if (App.Services.ReferralValidator is not null && App.Services.CurrentUserService?.CurrentUser is { } currentUser)
        {
            bool isValid = await App.Services.ReferralValidator.IsValidReferralAsync(ReferralCodeTextBox.Text, currentUser.Id);
            ReferralCodeTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
            ReferralCodeTextBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
        }
    }
}
