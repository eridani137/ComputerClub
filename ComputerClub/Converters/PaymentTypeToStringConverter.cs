using System.Globalization;
using System.Windows.Data;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Converters;

public class PaymentTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is PaymentType type
            ? type switch
            {
                PaymentType.TopUp => "Пополнение",
                PaymentType.Charge => "Списание",
                PaymentType.Refund => "Возврат",
                _ => string.Empty
            }
            : "Все";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}