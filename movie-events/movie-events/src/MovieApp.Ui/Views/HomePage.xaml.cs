using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Core.Models;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class HomePage : Page
{
    private bool _initialized;
    private readonly EventDialogContentBuilder _dialogBuilder;

    public HomeEventsViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = new HomeEventsViewModel(App.EventRepository);
        _dialogBuilder = new EventDialogContentBuilder(App.ReferralValidator, App.CurrentUserService);
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!_initialized)
        {
            _initialized = true;

            if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
            {
                string existingCode = await App.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
                if (string.IsNullOrEmpty(existingCode))
                {
                    MovieApp.Core.Services.ReferralCodeGenerator generator = new MovieApp.Core.Services.ReferralCodeGenerator();
                    string newCode = generator.Generate(currentUser.Username, currentUser.Id);
                    await App.AmbassadorRepository.CreateAmbassadorProfileAsync(currentUser.Id, newCode);
                }
            }
        }

        await EventCard.RefreshDiscountsAsync();
        await EventCard.RefreshJoinedEventIdsAsync();
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

        int? discountPercentage = EventCard.DiscountByEventId.TryGetValue(selectedEvent.Id, out int percentage) ? (int?)percentage : null;

        await _dialogBuilder.ShowEventDialogAsync(
            selectedEvent,
            XamlRoot,
            isJackpotEvent: false,
            discountPercentage: discountPercentage);
    }
}