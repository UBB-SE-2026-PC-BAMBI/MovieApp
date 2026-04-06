using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Infrastructure;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.ViewModels;
using Windows.System;

namespace MovieApp.Ui.Views;

public sealed partial class MainWindow : Window
{
    private readonly IEventRepository _eventRepository;

    public MainWindow(MainViewModel viewModel, IEventRepository eventRepository)
    {
        ViewModel = viewModel;
        _eventRepository = eventRepository;
        InitializeComponent();
        RootGrid.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnGlobalKeyDown), handledEventsToo: true);
        NavigateToRoute(AppRouteResolver.Home);
    }

    public MainViewModel ViewModel { get; }

    private void OnGlobalKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Space)
            return;

        if (ContentFrame.Content is SlotMachinePage slotPage &&
            slotPage.DataContext is SlotMachineViewModel vm &&
            vm.SpinCommand.CanExecute(null))
        {
            vm.SpinCommand.Execute(null);
            e.Handled = true;
        }
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.Activated -= MainWindow_Activated;
        await CheckForPriceDropsAsync();
    }

    private async Task CheckForPriceDropsAsync()
    {
        try
        {
            if (App.WatchlistPathProvider == null) return;

            string folderPath = App.WatchlistPathProvider.GetWatchlistFolderPath();
            LocalPriceWatcherRepository watcherRepo = new LocalPriceWatcherRepository(folderPath);
            IEnumerable<WatchedEvent> watchedEvents = await watcherRepo.GetAllWatchedEventsAsync();

            if (!watchedEvents.Any()) return;

            List<string> priceDroppedMessages = new List<string>();

            foreach (WatchedEvent watched in watchedEvents)
            {
                Event realEvent = await _eventRepository.FindByIdAsync(watched.EventId);

                if (realEvent != null && realEvent.TicketPrice <= watched.TargetPrice)
                {
                    priceDroppedMessages.Add($"Target reached! '{realEvent.Title}' is now {realEvent.TicketPrice:C} (Target: {watched.TargetPrice:C})");
                    await watcherRepo.RemoveWatchAsync(watched.EventId);
                }
            }

            if (priceDroppedMessages.Any())
            {
                PriceAlertInfoBar.Message = string.Join("\n", priceDroppedMessages);
                PriceAlertInfoBar.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error on startup check: {ex.Message}");
        }
    }

    public void NavigateToRoute(string tag)
    {
        Type pageType = AppRouteResolver.ResolvePageType(tag);
        SyncSelectedNavigationItem(tag);

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateToRoute(tag);
        }
    }

    private void SyncSelectedNavigationItem(string tag)
    {
        NavigationViewItem selectedItem = AppNavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .Concat(AppNavigationView.FooterMenuItems.OfType<NavigationViewItem>())
            .FirstOrDefault(item => string.Equals(item.Tag as string, tag, StringComparison.Ordinal));

        AppNavigationView.SelectedItem = selectedItem;
    }
}