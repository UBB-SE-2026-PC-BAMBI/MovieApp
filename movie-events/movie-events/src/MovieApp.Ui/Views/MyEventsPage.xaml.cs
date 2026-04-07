// <copyright file="MyEventsPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents the page that displays and manages the user's events and watchlist.
/// </summary>
public sealed partial class MyEventsPage : Page
{
    private bool initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyEventsPage"/> class.
    /// </summary>
    public MyEventsPage()
    {
        this.ViewModel = new MyEventsViewModel();
        this.InitializeComponent();
        this.DataContext = this.ViewModel;
        this.Loaded += this.MyEventsPage_Loaded;
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public MyEventsViewModel ViewModel { get; }

    /// <summary>
    /// Called when the page is navigated to and loads the user's watchlist.
    /// </summary>
    /// <param name="e">The navigation event data.</param>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await this.ViewModel.LoadWatchlistAsync();
    }

    /// <summary>
    /// Handles the page loaded event and initializes the view model once.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void MyEventsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        await this.InitializeViewModelAsync();
    }

    private async void WatchlistSaveButton_Click(object sender, RoutedEventArgs e)
    {
        await this.ViewModel.SaveSelectedWatchlistAsync();
    }

    private Task InitializeViewModelAsync()
    {
        return this.ViewModel.InitializeAsync();
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        this.ViewModel.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        this.ViewModel.SetSortOption(sortOption);
    }
}