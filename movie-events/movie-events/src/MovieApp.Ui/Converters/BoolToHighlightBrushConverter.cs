// <copyright file="BoolToHighlightBrushConverter.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Converters;

using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

/// <summary>
/// Converts a boolean to a highlight brush for jackpot events.
/// Returns a semi-transparent gold brush for true, transparent for false.
/// </summary>
public sealed class BoolToHighlightBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is true)
        {
            return new SolidColorBrush(Color.FromArgb(50, 255, 193, 7));
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}