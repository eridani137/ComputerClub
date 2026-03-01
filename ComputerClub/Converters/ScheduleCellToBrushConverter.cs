using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using ComputerClub.Models;

namespace ComputerClub.Converters;

public class ScheduleCellToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ScheduleCell cell) return Brushes.Transparent;
        return ComputerStatuses.GetCellColor(cell.IsOccupied, cell.IsReservation, cell.IsSelected, cell.IsPast);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}