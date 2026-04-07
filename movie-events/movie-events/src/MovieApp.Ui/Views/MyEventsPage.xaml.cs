// <copyright file="MyEventsPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Ui.ViewModels.Events;


public sealed partial class MyEventsPage : Page
{
    private bool _initialized;

    public MyEventsPage()
    {
        ViewModel = new MyEventsViewModel();
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += MyEventsPage_Loaded;
    }

    public MyEventsViewModel ViewModel { get; }

    private async void MyEventsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await InitializeViewModelAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadWatchlistAsync();
    }

    private async void WatchlistSaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveSelectedWatchlistAsync();
    }

    private Task InitializeViewModelAsync()
    {
        return ViewModel.InitializeAsync();
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
    }
}