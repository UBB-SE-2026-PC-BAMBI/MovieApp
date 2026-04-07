// <copyright file="FavoritesPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MovieApp.Ui.ViewModels;

/// <summary>
/// Hosts the current user's persisted favorite events.
/// </summary>
public sealed partial class FavoritesPage : Page
{
    /// <summary>
    /// Indicates whether the page has already been initialized.
    /// </summary>
    private bool initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesPage"/> class.
    /// </summary>
    public FavoritesPage()
    {
        this.ViewModel = new FavoritesViewModel();
        this.InitializeComponent();
        this.DataContext = this.ViewModel;
        this.Loaded += this.FavoritesPage_Loaded;
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public FavoritesViewModel ViewModel { get; }

    private async void FavoritesPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        await this.ViewModel.InitializeAsync();
    }
}
