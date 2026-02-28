using System.Globalization;
using System.Windows.Data;

namespace ComputerClub.Converters;

public class UtcToLocalTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not DateTime utcDateTime
            ? value
            : utcDateTime.ToLocalTime();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not DateTime localDateTime 
            ? value 
            : localDateTime.ToUniversalTime();
    }
}