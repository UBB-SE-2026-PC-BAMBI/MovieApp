using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MovieApp.Core.Models;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

namespace MovieApp.Ui.Views;

/// <summary>
/// Exposes the referral-code feature area, including ambassador progress,
/// usage tracking, and reward-threshold handoff points.
/// </summary>
public sealed partial class ReferralAreaPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _referralCode = string.Empty;
    public string ReferralCode
    {
        get => _referralCode;
        set
        {
            if (_referralCode != value)
            {
                _referralCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReferralCode)));
            }
        }
    }

    public ObservableCollection<ReferralHistoryItem> ReferralHistory { get; } = new();

    public Visibility IsHistoryEmpty => ReferralHistory.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public ReferralAreaPage()
    {
        InitializeComponent();
        ReferralHistory.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHistoryEmpty)));
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            var code = await App.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
            ReferralCode = code ?? "No code generated";

            var history = await App.AmbassadorRepository.GetReferralHistoryAsync(currentUser.Id);
            ReferralHistory.Clear();
            foreach (var item in history)
            {
                ReferralHistory.Add(item);
            }
        }
    }
}
