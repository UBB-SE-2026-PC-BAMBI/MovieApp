// <copyright file="SeatGuideDialog.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Controls;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents a dialog that displays a seat guide for an event.
/// </summary>
public sealed partial class SeatGuideDialog : ContentDialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeatGuideDialog"/> class.
    /// </summary>
    /// <param name="totalCapacity">The total seating capacity used to generate the layout.</param>
    public SeatGuideDialog(int totalCapacity)
    {
        this.InitializeComponent();

        this.ViewModel = new SeatGuideViewModel(totalCapacity);
        this.DataContext = this.ViewModel;
    }

    /// <summary>
    /// Gets the view model associated with the dialog.
    /// </summary>
    /// <returns>The <see cref="SeatGuideViewModel"/> instance.</returns>
    public SeatGuideViewModel ViewModel { get; }

    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns>
    /// <see cref="Visibility.Visible"/> if <paramref name="value"/> is <c>true</c>;
    /// otherwise, <see cref="Visibility.Collapsed"/>.
    /// </returns>
    public static Visibility GetVisibility(bool value)
        => value ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Converts a boolean value to the inverse <see cref="Visibility"/> value.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns>
    /// <see cref="Visibility.Collapsed"/> if <paramref name="value"/> is <c>true</c>;
    /// otherwise, <see cref="Visibility.Visible"/>.
    /// </returns>
    public static Visibility GetInverseVisibility(bool value)
        => value ? Visibility.Collapsed : Visibility.Visible;
}