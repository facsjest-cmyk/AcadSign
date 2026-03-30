using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AcadSign.Desktop.Converters;

public class DongleStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EC9B0"))  // Vert
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E")); // Gris
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class DongleStatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EC9B020")) // Vert transparent
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E20")); // Gris transparent
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E20"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class DongleStatusToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EC9B0"))  // Vert
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E")); // Gris
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class DongleStatusToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? "Connecté" : "Hors ligne";
        }
        return "Hors ligne";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
