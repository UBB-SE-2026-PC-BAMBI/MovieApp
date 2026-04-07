// <copyright file="EventManagementPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MovieApp.Core.EventLists;
using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents the event management page, providing functionality for viewing, searching,
/// sorting, and editing events.
/// </summary>
public sealed partial class EventManagementPage : Page
{
    /// <summary>
    /// Indicates whether the page has already been initialized.
    /// </summary>
    private bool initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventManagementPage"/> class.
    /// </summary>
    public EventManagementPage()
    {
        this.ViewModel = new EventManagementViewModel();
        this.InitializeComponent();
        this.DataContext = this.ViewModel;
        this.Loaded += this.EventManagementPage_Loaded;
    }

    /// <summary>
    /// Gets the view model associated with this page.
    /// </summary>
    public EventManagementViewModel ViewModel { get; }

    /// <summary>
    /// Handles the page loaded event and initializes the view model once.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void EventManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        await this.ViewModel.InitializeAsync();
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        this.ViewModel.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, EventSortOption sortOption)
    {
        this.ViewModel.SetSortOption(sortOption);
    }

    private void EventListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.ViewModel.SelectedEvent is not Event selected)
        {
            return;
        }

        this.ViewModel.FormTitle = selected.Title;
        this.ViewModel.FormLocation = selected.LocationReference;
        this.ViewModel.FormEventType = selected.EventType;
        this.ViewModel.FormDescription = selected.Description ?? string.Empty;
        this.ViewModel.FormDate = new DateTimeOffset(selected.EventDateTime);
        this.ViewModel.FormTime = selected.EventDateTime.TimeOfDay;
        this.ViewModel.FormPrice = (double)selected.TicketPrice;
        this.ViewModel.FormCapacity = selected.MaxCapacity;
        this.ViewModel.FormPosterUrl = selected.PosterUrl ?? string.Empty;

        this.Bindings.Update();
    }
}