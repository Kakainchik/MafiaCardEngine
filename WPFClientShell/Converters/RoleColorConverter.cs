using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WPFClientShell.Extensions;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.Converters
{
    public class RoleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var standart = new SolidColorBrush();
            if(targetType == typeof(Brush) && value != null && value is RoleVisual role)
            {
                return role.GetColor();
            }
            return standart;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}