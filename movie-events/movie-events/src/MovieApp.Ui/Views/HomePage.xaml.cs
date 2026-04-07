// <copyright file="HomePage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;


public sealed partial class HomePage : Page
{
    private bool _initialized;
    private readonly EventDialogContentBuilder _dialogBuilder;

    public HomeEventsViewModel ViewModel { get; }

    public HomePage()
    {
        if (App.Services.EventRepository == null || App.Services.ReferralValidator == null)
            throw new InvalidOperationException("Required repositories are not initialized.");

        ViewModel = new HomeEventsViewModel(App.Services.EventRepository);
        _dialogBuilder = new EventDialogContentBuilder(App.Services.ReferralValidator, App.Services.CurrentUserService);
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private bool IsAmbassadorSystemAvailable()
    {
        return App.Services.AmbassadorRepository is not null && App.Services.CurrentUserService?.CurrentUser is not null;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        App.EnsureServicesValid();

        if (!_initialized)
        {
            _initialized = true;

            if (IsAmbassadorSystemAvailable())
            {
                var currentUser = App.Services.CurrentUserService!.CurrentUser;
                string existingCode = await App.Services.AmbassadorRepository!.GetReferralCodeAsync(currentUser.Id);
                if (string.IsNullOrEmpty(existingCode))
                {
                    MovieApp.Core.Services.ReferralCodeGenerator generator = new MovieApp.Core.Services.ReferralCodeGenerator();
                    string newCode = generator.Generate(currentUser.Username, currentUser.Id);
                    await App.Services.AmbassadorRepository.CreateAmbassadorProfileAsync(currentUser.Id, newCode);
                }
            }
        }

        await ViewModel.InitializeAsync();
    }

    private void ShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not HomeNavigationShortcut shortcut)
        {
            return;
        }

        if (App.CurrentMainWindow is not null)
        {
            App.CurrentMainWindow.NavigateToRoute(shortcut.RouteTag);
            return;
        }

        Frame.Navigate(AppRouteResolver.ResolvePageType(shortcut.RouteTag));
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
    }

    private async void EventCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        int? discountPercentage = null;

        if (App.Services.EventUserStateService is not null)
        {
            int percentage = await App.Services.EventUserStateService.GetDiscountForEventAsync(selectedEvent.Id);
            if (percentage > 0)
            {
                discountPercentage = percentage;
            }
        }

        ContentDialog dialog = await _dialogBuilder.BuildDialogAsync(
            selectedEvent,
            XamlRoot,
            isJackpotEvent: false,
            discountPercentage: discountPercentage);

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