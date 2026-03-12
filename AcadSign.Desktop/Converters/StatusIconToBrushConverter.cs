using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AcadSign.Desktop.Converters;

public class StatusIconToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var icon = value as string;

        return icon switch
        {
            "✅" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EC9B0")),
            "⚠️" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CE9178")),
            "❌" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F48771")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E3E42"))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
