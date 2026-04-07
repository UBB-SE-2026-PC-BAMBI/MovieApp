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

/// <summary>
/// Represents a page that displays events for a specific section,
/// providing filtering, sorting, and interaction capabilities.
/// </summary>
public sealed partial class SectionEventsPage : Page
{
    private readonly EventDialogContentBuilder dialogBuilder;

    private bool initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="SectionEventsPage"/> class.
    /// </summary>
    public SectionEventsPage()
    {
        this.dialogBuilder = new EventDialogContentBuilder(
            App.Services.ReferralValidator,
            App.Services.CurrentUserService);

        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public SectionEventsViewModel? ViewModel { get; private set; }

    /// <summary>
    /// Called when the page is navigated to and initializes the view model for the selected section.
    /// </summary>
    /// <param name="e">The navigation event data.</param>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (this.initialized)
        {
            return;
        }

        if (e.Parameter is not SectionNavigationContext context)
        {
            return;
        }

        this.ViewModel = new SectionEventsViewModel(App.Services.EventRepository, context);
        this.DataContext = this.ViewModel;

        this.initialized = true;
        await this.ViewModel.InitializeAsync();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        this.ViewModel?.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        this.ViewModel?.SetSortOption(sortOption);
    }

    private async void EventCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        ContentDialog dialog = await this.dialogBuilder.BuildDialogAsync(
            selectedEvent,
            this.XamlRoot,
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