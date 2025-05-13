using System;
namespace RecreationBookingApp.Converters;
public class IntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value?.ToString() ?? "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out int result))
            return result;
        return 1; // Значение по умолчанию
    }
}