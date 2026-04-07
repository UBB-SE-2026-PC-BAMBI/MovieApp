// <copyright file="WinUiDialogService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

/// <summary>
/// WinUI implementation of <see cref="IDialogService"/> backed by <see cref="ContentDialog"/>.
/// </summary>
public sealed class WinUiDialogService : IDialogService
{
    private Microsoft.UI.Xaml.XamlRoot? xamlRoot;

    /// <inheritdoc/>
    public void SetXamlRoot(Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        this.xamlRoot = xamlRoot ?? throw new ArgumentNullException(nameof(xamlRoot));
    }

    /// <inheritdoc/>
    public async Task ShowInfoAsync(string title, string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = this.GetXamlRoot(),
            Title = title,
            Content = message,
            CloseButtonText = "OK",
        };

        await dialog.ShowAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> ShowConfirmAsync(string title, string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = this.GetXamlRoot(),
            Title = title,
            Content = message,
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
        };

        ContentDialogResult result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private Microsoft.UI.Xaml.XamlRoot GetXamlRoot()
    {
        if (this.xamlRoot is null)
        {
            throw new InvalidOperationException(
                "XamlRoot has not been set. Call SetXamlRoot before showing any dialog.");
        }

        return this.xamlRoot;
    }
}