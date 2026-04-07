// <copyright file="MainWindow.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Infrastructure;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.ViewModels;
using Windows.System;

/// <summary>
/// Represents the main application window and handles global navigation,
/// keyboard shortcuts, and background checks such as price drop notifications.
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Provides access to event data.
    /// </summary>
    private readonly IEventRepository eventRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The main view model for the application.</param>
    /// <param name="eventRepository">The repository used to access event data.</param>
    public MainWindow(MainViewModel viewModel, IEventRepository eventRepository)
    {
        this.ViewModel = viewModel;
        this.eventRepository = eventRepository;
        this.InitializeComponent();
        this.RootGrid.AddHandler(
            UIElement.KeyDownEvent,
            new KeyEventHandler(this.OnGlobalKeyDown),
            handledEventsToo: true);

        this.NavigateToRoute(AppRouteResolver.Home);
    }

    /// <summary>
    /// Gets the main view model associated with this window.
    /// </summary>
    public MainViewModel ViewModel { get; }

    /// <summary>
    /// Navigates to a page associated with the specified route tag.
    /// </summary>
    /// <param name="tag">The route identifier.</param>
    public void NavigateToRoute(string tag)
    {
        Type pageType = AppRouteResolver.ResolvePageType(tag);
        this.SyncSelectedNavigationItem(tag);

        if (this.ContentFrame.CurrentSourcePageType != pageType)
        {
            this.ContentFrame.Navigate(pageType);
        }
    }

    /// <summary>
    /// Handles global key down events and triggers slot machine spin on space key press.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnGlobalKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Space)
        {
            return;
        }

        if (this.ContentFrame.Content is SlotMachinePage slotPage &&
            slotPage.DataContext is SlotMachineViewModel slotMachineViewModel &&
            slotMachineViewModel.SpinCommand.CanExecute(null))
        {
            slotMachineViewModel.SpinCommand.Execute(null);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles the window activation event and triggers startup checks.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The activation event data.</param>
    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.Activated -= this.MainWindow_Activated;
        await this.CheckForPriceDropsAsync();
    }

    /// <summary>
    /// Checks for events whose prices have dropped below the user's target and notifies the user.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CheckForPriceDropsAsync()
    {
        try
        {
            if (App.Services.WatchlistPathProvider == null)
            {
                return;
            }

            string folderPath = App.Services.WatchlistPathProvider.GetWatchlistFolderPath();
            LocalPriceWatcherRepository watcherRepo = new LocalPriceWatcherRepository(folderPath);
            IEnumerable<WatchedEvent> watchedEvents = await watcherRepo.GetAllWatchedEventsAsync();

            if (!watchedEvents.Any())
            {
                return;
            }

            List<string> priceDroppedMessages = new List<string>();

            foreach (WatchedEvent watched in watchedEvents)
            {
                Event? realEvent = await this.eventRepository.FindByIdAsync(watched.EventId);

                if (realEvent != null && realEvent.TicketPrice <= watched.TargetPrice)
                {
                    priceDroppedMessages.Add($"Target reached! '{realEvent.Title}' is now {realEvent.TicketPrice:C} (Target: {watched.TargetPrice:C})");
                    await watcherRepo.RemoveWatchAsync(watched.EventId);
                }
            }

            if (priceDroppedMessages.Any())
            {
                this.PriceAlertInfoBar.Message = string.Join("\n", priceDroppedMessages);
                this.PriceAlertInfoBar.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error on startup check: {ex.Message}");
        }
    }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            this.NavigateToRoute(tag);
        }
    }

    private void SyncSelectedNavigationItem(string tag)
    {
        if (this.AppNavigationView is null || this.AppNavigationView.MenuItems is null)
        {
            throw new NullReferenceException("NavigationView menu items are not initialized.");
        }

        NavigationViewItem? selectedItem = this.AppNavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .Concat(this.AppNavigationView.FooterMenuItems.OfType<NavigationViewItem>())
            .FirstOrDefault(item => string.Equals(item.Tag as string, tag, StringComparison.Ordinal));

        this.AppNavigationView.SelectedItem = selectedItem;
    }
}