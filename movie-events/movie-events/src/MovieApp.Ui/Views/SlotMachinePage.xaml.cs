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
        SolidColorBrush gold = new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0x97, 0x1A));
        SolidColorBrush darkBg = new SolidColorBrush(Color.FromArgb(0xFF, 0x0B, 0x0B, 0x1E));
        SolidColorBrush lightText = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xFF));

        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            DefaultButton = ContentDialogButton.Primary,
        };

        dialog.Resources["ContentDialogBackground"] = darkBg;
        dialog.Resources["ContentDialogForeground"] = lightText;
        dialog.Resources["ContentDialogTitleForeground"] = gold;
        dialog.Resources["ContentDialogBorderBrush"] = gold;

        JackpotDialogViewModel model = new JackpotDialogViewModel(movie, discountPercentage)
        {
            CollectAction = () => dialog.Hide()
        };

        dialog.ContentTemplate = (DataTemplate)Resources["JackpotDialogTemplate"];
        dialog.Content = model;

        dialog.PrimaryButtonText = "Collect!";
        dialog.PrimaryButtonClick += (_, _) => model.CollectAction?.Invoke();

        await dialog.ShowAsync();
        this.Focus(FocusState.Programmatic);
    }

    private async void ViewEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not MatchingEventItem item)
            return;

        Event selectedEvent = item.Event;
        int discountPct = item.IsJackpotEvent
            ? (EventCard.DiscountByEventId.TryGetValue(selectedEvent.Id, out int pct) ? pct : 70)
            : 0;

        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = selectedEvent.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
        };

        EventDialogViewModel model = await _dialogBuilder.BuildAsync(
            selectedEvent,
            isJackpotEvent: item.IsJackpotEvent,
            discountPercent: item.IsJackpotEvent ? discountPct : null,
            closeDialogAction: () => dialog.Hide(),
            xamlRoot: XamlRoot);

        dialog.Content = EventDialogViewBuilder.Create(model);

        await dialog.ShowAsync();
        this.Focus(FocusState.Programmatic);

        if (DataContext is SlotMachineViewModel vm)
            await vm.RefreshSpinCountAsync();
    }
}