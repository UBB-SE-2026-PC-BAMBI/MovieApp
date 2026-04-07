// <copyright file="IDialogService.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Services;

using System.Threading.Tasks;

/// <summary>
/// Abstracts UI dialog presentation to keep pages testable and free of dialog infrastructure.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an informational dialog with a single OK button.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <returns>A task that completes when the dialog is dismissed.</returns>
    Task ShowInfoAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog with OK and Cancel buttons.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <returns>True if the user confirmed, false if cancelled.</returns>
    Task<bool> ShowConfirmAsync(string title, string message);

    /// <summary>
    /// Sets the XamlRoot required by ContentDialog. Call this from the page's Loaded event
    /// or constructor before any dialog is shown.
    /// </summary>
    /// <param name="xamlRoot">The XamlRoot from the hosting page.</param>
    void SetXamlRoot(Microsoft.UI.Xaml.XamlRoot xamlRoot);
}