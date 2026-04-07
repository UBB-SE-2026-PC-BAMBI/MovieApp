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
    /// <summary>
    /// Initializes a new instance of the <see cref="DetailsCheckoutPage"/> class.
    /// </summary>
    public DetailsCheckoutPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Validates the referral code entered by the user and updates the UI accordingly.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void ValidateReferralCodeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.ReferralCodeTextBox.Text))
        {
            return;
        }

        if (App.Services.ReferralValidator is not null &&
            App.Services.CurrentUserService?.CurrentUser is { } currentUser)
        {
            bool isValid = await App.Services.ReferralValidator
                .IsValidReferralAsync(this.ReferralCodeTextBox.Text, currentUser.Id);
            this.ReferralCodeTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
            this.ReferralCodeTextBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
        }
    }
}
