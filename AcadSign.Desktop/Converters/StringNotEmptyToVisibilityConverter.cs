using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AcadSign.Desktop.Converters;

public class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter is string p
            && string.Equals(p, "invert", StringComparison.OrdinalIgnoreCase);

        if (value is string s)
        {
            var vis = string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
            if (!invert)
                return vis;

            return vis == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        return invert ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
