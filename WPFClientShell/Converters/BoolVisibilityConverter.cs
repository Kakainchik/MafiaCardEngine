using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace WPFClientShell.Converters
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? vis = (bool?)value;
            if(vis.HasValue)
            {
                return vis.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch((Visibility)value)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Collapsed:
                    return false;
                case Visibility.Hidden:
                    return null;
                default: return false;
            };
        }
    }
}