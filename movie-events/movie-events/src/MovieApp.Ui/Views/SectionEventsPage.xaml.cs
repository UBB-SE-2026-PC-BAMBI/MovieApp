// <copyright file="SectionEventsPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;


public sealed partial class SectionEventsPage : Page
{
    private bool _initialized;
    private readonly EventDialogContentBuilder _dialogBuilder;

    public SectionEventsViewModel? ViewModel { get; private set; }

    public SectionEventsPage()
    {
        _dialogBuilder = new EventDialogContentBuilder(App.Services.ReferralValidator, App.Services.CurrentUserService);
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (_initialized)
        {
            return;
        }

        if (e.Parameter is not SectionNavigationContext context)
        {
            return;
        }

        ViewModel = new SectionEventsViewModel(App.Services.EventRepository, context);
        DataContext = ViewModel;

        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel?.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel?.SetSortOption(sortOption);
    }

    private async void EventCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        ContentDialog dialog = await _dialogBuilder.BuildDialogAsync(
            selectedEvent,
            XamlRoot,
            isJackpotEvent: false,
            discountPercentage: null);

        await dialog.ShowAsync();
    }

    private async void JoinEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        button.IsEnabled = false;

        if (App.Services.EventJoinService is not null)
        {
            string tag = button.Tag?.ToString() ?? string.Empty;
            JoinEventResult result = await App.Services.EventJoinService.JoinEventAsync(selectedEvent.Id, tag);
            button.Content = result.Message;
        }
    }
}