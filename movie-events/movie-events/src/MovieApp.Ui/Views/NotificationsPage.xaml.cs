// <copyright file="NotificationsPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MovieApp.Ui.ViewModels;

/// <summary>
/// Hosts the current user's event-related notifications.
/// </summary>
public sealed partial class NotificationsPage : Page
{
    private bool initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsPage"/> class.
    /// </summary>
    public NotificationsPage()
    {
        this.ViewModel = new NotificationsViewModel();
        this.InitializeComponent();
        this.DataContext = this.ViewModel;
        this.Loaded += this.NotificationsPage_Loaded;
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public NotificationsViewModel ViewModel { get; }

    private async void NotificationsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        await this.ViewModel.InitializeAsync();
    }
}
