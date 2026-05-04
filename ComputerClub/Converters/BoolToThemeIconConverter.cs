using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace ComputerClub.Converters
{
    public class BoolToThemeIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true
                ? new SymbolIcon(SymbolRegular.WeatherSunny24)
                : new SymbolIcon(SymbolRegular.WeatherMoon24);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
