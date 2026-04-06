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
            App.SlotMachineService ?? throw new System.InvalidOperationException("SlotMachineService not initialized"),
            App.SlotMachineAnimationService ?? throw new System.InvalidOperationException("SlotMachineAnimationService not initialized"));

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

        int discountPercentage = 0;

        if (item.IsJackpotEvent)
        {
            if (App.EventUserStateService is not null)
            {
                int percentage = await App.EventUserStateService.GetDiscountForEventAsync(selectedEvent.Id);
                discountPercentage = percentage > 0 ? percentage : 70;
            }
            else
            {
                discountPercentage = 70;
            }
        }

        ContentDialog dialog = await _dialogBuilder.BuildDialogAsync(
            selectedEvent,
            XamlRoot,
            isJackpotEvent: item.IsJackpotEvent,
            discountPercentage: item.IsJackpotEvent ? discountPercentage : null);

        await dialog.ShowAsync();

        this.Focus(FocusState.Programmatic);

        if (DataContext is SlotMachineViewModel slotMachineViewModel)
            await slotMachineViewModel.RefreshSpinCountAsync();
    }
}