using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Core.Models;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class SectionEventsPage : Page
{
    private bool _initialized;
    private readonly EventDialogContentBuilder _dialogBuilder;

    public SectionEventsViewModel? ViewModel { get; private set; }

    public SectionEventsPage()
    {
        _dialogBuilder = new EventDialogContentBuilder(App.ReferralValidator, App.CurrentUserService);
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

        ViewModel = new SectionEventsViewModel(App.EventRepository, context);
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

        await _dialogBuilder.ShowEventDialogAsync(
            selectedEvent,
            XamlRoot,
            isJackpotEvent: false,
            discountPercentage: null);
    }
}