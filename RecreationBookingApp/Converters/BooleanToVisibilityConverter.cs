using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace RecreationBookingApp.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = (bool)value;
        if (parameter?.ToString() == "inverse")
            isVisible = !isVisible;
        return isVisible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}