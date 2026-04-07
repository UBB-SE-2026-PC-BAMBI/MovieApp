// <copyright file="NegativeBooleanConverter.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.Converters;

using System;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// Inverts a boolean value for use in XAML bindings.
/// Converts true to false and false to true.
/// </summary>
public sealed class NegativeBooleanConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return value;
    }
}