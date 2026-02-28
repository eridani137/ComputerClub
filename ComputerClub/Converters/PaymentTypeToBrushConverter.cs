using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Converters;

public class PaymentTypeToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is PaymentType type
            ? type switch
            {
                PaymentType.Charge => new SolidColorBrush(Color.FromRgb(196, 43, 28)),
                PaymentType.Refund => new SolidColorBrush(Color.FromRgb(157, 130, 0)),
                _ => new SolidColorBrush(Color.FromRgb(15, 140, 64))
            }
            : Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}