using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;

namespace ComputerClub.Converters;

public class PcTypeToBrushConverter : IValueConverter
{
    private Brush DefaultBrush { get; } = Brushes.Gray;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int type) return DefaultBrush;

        return type switch
        {
            0 => DefaultBrush,
            1 => Brushes.IndianRed,
            2 => Brushes.DarkOrange,
            3 => Brushes.Yellow,
            4 => Brushes.PaleGreen,
            6 => Brushes.Violet,
            _ => DefaultBrush
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}