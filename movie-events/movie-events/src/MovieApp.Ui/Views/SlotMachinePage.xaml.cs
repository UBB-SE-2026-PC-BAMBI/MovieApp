using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels;
using Windows.UI;

namespace MovieApp.Ui.Views;

public sealed partial class SlotMachinePage : Page
{
    private readonly EventDialogContentBuilder _dialogBuilder;

    public SlotMachinePage()
    {
        _dialogBuilder = new EventDialogContentBuilder(App.ReferralValidator, App.CurrentUserService);
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;

        User? currentUser = App.CurrentUserService?.CurrentUser;
        if (currentUser is null)
            return;

        SlotMachineViewModel viewModel = new SlotMachineViewModel(
            currentUser.Id,
            App.SlotMachineService ?? throw new InvalidOperationException("SlotMachineService not initialized"),
            App.SlotMachineAnimationService ?? throw new InvalidOperationException("SlotMachineAnimationService not initialized"));

        viewModel.JackpotHit += OnJackpotHit;
        DataContext = viewModel;
        await viewModel.InitializeAsync();
        this.Focus(FocusState.Programmatic);
    }

    private async void OnJackpotHit(Movie movie, int discountPercentage)
    {
        ContentDialog dialog = (ContentDialog)Resources["JackpotContentDialog"];
        dialog.XamlRoot = XamlRoot;

        JackpotDialogViewModel jackpotViewModel = new JackpotDialogViewModel(movie, discountPercentage);

        dialog.Content = jackpotViewModel;
        await dialog.ShowAsync();

        this.Focus(FocusState.Programmatic);
    }

    private async void OpenEventDetailsDialog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not MatchingEventItem item)
            return;

        Event selectedEvent = item.Event;

        int discountPercentage = item.IsJackpotEvent
            ? (EventCard.DiscountByEventId.TryGetValue(selectedEvent.Id, out int percentage) ? percentage : 70)
            : 0;

        await _dialogBuilder.ShowEventDialogAsync(
            selectedEvent,
            XamlRoot,
            isJackpotEvent: item.IsJackpotEvent,
            discountPercentage: item.IsJackpotEvent ? discountPercentage : null);

        this.Focus(FocusState.Programmatic);

        if (DataContext is SlotMachineViewModel slotMachineViewModel)
            await slotMachineViewModel.RefreshSpinCountAsync();
    }
}