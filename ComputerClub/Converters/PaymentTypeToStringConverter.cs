using System.Globalization;
using System.Windows.Data;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Converters;

public class PaymentTypeToStringConverter : IValueConverter
{
    public static string Convert(PaymentType type) => type switch
    {
        PaymentType.TopUpCash => "Пополнение (наличные)",
        PaymentType.TopUpCard => "Пополнение (карта)",
        PaymentType.Charge => "Списание",
        PaymentType.Refund => "Возврат",
        _ => type.ToString()
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is PaymentType type ? Convert(type) : "Все";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}