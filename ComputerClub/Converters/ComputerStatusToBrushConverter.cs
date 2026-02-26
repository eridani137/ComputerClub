using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub.Converters;

public class ComputerStatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is ComputerStatus status 
            ? ComputerStatuses.GetByStatus(status).Color 
            : Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}