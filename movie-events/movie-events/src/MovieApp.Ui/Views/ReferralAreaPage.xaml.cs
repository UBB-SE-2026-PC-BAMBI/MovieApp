// <copyright file="ReferralAreaPage.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Views;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;

/// <summary>
/// Exposes the referral-code feature area, including ambassador progress,
/// usage tracking, and reward-threshold handoff points.
/// </summary>
public sealed partial class ReferralAreaPage : Page, INotifyPropertyChanged
{
    private string referralCode = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferralAreaPage"/> class.
    /// </summary>
    public ReferralAreaPage()
    {
        this.InitializeComponent();
        this.ReferralHistory.CollectionChanged += (_, _) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsHistoryEmpty)));
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the referral code associated with the current user.
    /// </summary>
    public string ReferralCode
    {
        get => this.referralCode;
        set
        {
            if (this.referralCode != value)
            {
                this.referralCode = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ReferralCode)));
            }
        }
    }

    /// <summary>
    /// Gets the collection of referral history items.
    /// </summary>
    public ObservableCollection<ReferralHistoryItem> ReferralHistory { get; } = new ();

    /// <summary>
    /// Gets a value indicating whether the referral history is empty.
    /// </summary>
    public Visibility IsHistoryEmpty =>
        this.ReferralHistory.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Called when the page is navigated to and loads referral data for the current user.
    /// </summary>
    /// <param name="e">The navigation event data.</param>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (App.Services.AmbassadorRepository is not null && App.Services.CurrentUserService?.CurrentUser is { } currentUser)
        {
            string? code = await App.Services.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
            this.ReferralCode = code ?? "No code generated";

            IEnumerable<ReferralHistoryItem> history =
                await App.Services.AmbassadorRepository.GetReferralHistoryAsync(currentUser.Id);

            this.ReferralHistory.Clear();
            foreach (ReferralHistoryItem item in history)
            {
                this.ReferralHistory.Add(item);
            }
        }
    }
}
