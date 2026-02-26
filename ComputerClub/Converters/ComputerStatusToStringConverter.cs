using System.Globalization;
using System.Windows.Data;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Converters;

public class ComputerStatusToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ComputerStatus status
            ? status switch
            {
                ComputerStatus.Available => "Свободен",
                ComputerStatus.Occupied => "Занят",
                ComputerStatus.Reserved => "Забронирован",
                ComputerStatus.OutOfService => "Не работает",
                _ => string.Empty
            }
            : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}