using Microsoft.UI.Xaml.Data;

namespace MovieApp.Ui.Converters;

/// <summary>
/// Inverts a boolean value for use in XAML bindings.
/// Converts true to false and false to true.
/// </summary>
public sealed class NegativeBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
            return !boolValue;

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
            return !boolValue;

        return value;
    }
}
