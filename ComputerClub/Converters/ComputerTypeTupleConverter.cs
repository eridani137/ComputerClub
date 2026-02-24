using System.Globalization;
using System.Windows.Data;
using ComputerClub.Models;

namespace ComputerClub.Converters;

public class ComputerTypeTupleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is ComputerTypeDefinition type &&
            values[1] is ComputerCanvasItem computerCanvasItem)
        {
            return (computerCanvasItem, type);
        }

        return Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}