using System.Globalization;
using System.Windows.Data;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Converters;

public class PaymentTypeEqualConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PaymentType current && parameter is string paramStr
            && Enum.TryParse<PaymentType>(paramStr, out var target))
        {
            return current == target;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string paramStr
            && Enum.TryParse<PaymentType>(paramStr, out var target))
        {
            return target;
        }
        return Binding.DoNothing;
    }
}
