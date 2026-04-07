// <copyright file="EventSearchBox.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Controls;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
/// Reusable free-text search input for event-list screens.
/// </summary>
/// <remarks>
/// This control is UI-only. It exposes the current text and a text-changed event so
/// each screen can apply the shared event-list search behavior to its own view model.
/// </remarks>
public sealed partial class EventSearchBox : UserControl
{
    /// <summary>
    /// Identifies the <see cref="SearchText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(EventSearchBox),
            new PropertyMetadata(string.Empty, OnSearchTextChanged));

    /// <summary>
    /// Identifies the <see cref="PlaceholderText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(EventSearchBox),
            new PropertyMetadata("Search events"));

    private bool isLoaded;
    private string pendingSearchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSearchBox"/> class.
    /// </summary>
    public EventSearchBox()
    {
        this.InitializeComponent();
        this.Loaded += this.OnLoaded;
    }

    /// <summary>
    /// Raised whenever the search text changes through user interaction or property sync.
    /// </summary>
    public event EventHandler<string>? SearchTextChanged;

    /// <summary>
    /// Gets or sets the current search text shown by the control.
    /// </summary>
    public string SearchText
    {
        get => (string)this.GetValue(SearchTextProperty);
        set => this.SetValue(SearchTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text displayed when no search query is entered.
    /// </summary>
    public string PlaceholderText
    {
        get => (string)this.GetValue(PlaceholderTextProperty);
        set => this.SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Handles changes to the <see cref="SearchText"/> dependency property.
    /// Updates the internal pending value and applies it to the UI.
    /// </summary>
    /// <param name="dependencyObject">The dependency object instance.</param>
    /// <param name="args">The property changed event arguments.</param>
    private static void OnSearchTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not EventSearchBox searchBox)
        {
            return;
        }

        searchBox.pendingSearchText = args.NewValue as string ?? string.Empty;
        searchBox.ApplySearchText(searchBox.pendingSearchText);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.isLoaded = true;
        this.ApplySearchText(this.pendingSearchText);
    }

    private void ApplySearchText(string text)
    {
        if (!this.isLoaded || this.SearchTextBox is null)
        {
            return;
        }

        if (this.SearchTextBox.Text != text)
        {
            this.SearchTextBox.Text = text;
        }
    }

    /// <summary>
    /// Synchronizes the current text into the dependency property and notifies the host page.
    /// </summary>
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string newValue = this.SearchTextBox.Text ?? string.Empty;
        if (!string.Equals(this.SearchText, newValue, StringComparison.Ordinal))
        {
            this.SearchText = newValue;
        }

        this.SearchTextChanged?.Invoke(this, newValue);
    }
}
