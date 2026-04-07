// <copyright file="SectionHeader.xaml.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Controls;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents a reusable section header with a title and optional action text.
/// </summary>
public sealed partial class SectionHeader : UserControl
{
    /// <summary>
    /// Identifies the <see cref="Title"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SectionHeader),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the <see cref="ActionText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActionTextProperty =
        DependencyProperty.Register(
            nameof(ActionText),
            typeof(string),
            typeof(SectionHeader),
            new PropertyMetadata("See all"));

    /// <summary>
    /// Initializes a new instance of the <see cref="SectionHeader"/> class.
    /// </summary>
    public SectionHeader()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the title displayed in the section header.
    /// </summary>
    /// <returns>The section title text.</returns>
    public string Title
    {
        get => (string)this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the action text displayed alongside the title.
    /// </summary>
    /// <returns>The action text.</returns>
    public string ActionText
    {
        get => (string)this.GetValue(ActionTextProperty);
        set => this.SetValue(ActionTextProperty, value);
    }
}