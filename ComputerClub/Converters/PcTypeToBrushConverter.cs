using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;

namespace ComputerClub.Converters;

public class PcTypeToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int typeId) return Brushes.Gray;
        return PcTypes.GetById(typeId).Color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}