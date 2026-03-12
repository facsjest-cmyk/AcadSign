using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AcadSign.Desktop.Converters;

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not Visibility.Visible;
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value == null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InverseNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value == null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class ViewModeToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "Vue: Après signature" : "Vue: Avant signature";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class DoubleToPercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? $"{(int)(d * 100)}%" : "100%";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        var color = s?.ToUpperInvariant() switch
        {
            "SIGNED" => "#10B981",
            "EMAIL_SENT" => "#6366F1",
            "ERROR" => "#EF4444",
            "FAILED" => "#EF4444",
            "SIGNING" => "#F59E0B",
            _ => "#F59E0B"
        };
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        var color = s?.ToUpperInvariant() switch
        {
            "SIGNED" => "#10B981",
            "EMAIL_SENT" => "#6366F1",
            "ERROR" => "#EF4444",
            "FAILED" => "#EF4444",
            "SIGNING" => "#F59E0B",
            _ => "#F59E0B"
        };
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        var color = s?.ToUpperInvariant() switch
        {
            "SIGNED" => "#10B98118",
            "EMAIL_SENT" => "#6366F118",
            "ERROR" => "#EF444415",
            "FAILED" => "#EF444415",
            "SIGNING" => "#F59E0B18",
            _ => "#F59E0B18"
        };
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        return s?.ToUpperInvariant() switch
        {
            "SIGNED" => "✓ Signé",
            "EMAIL_SENT" => "📧 Email envoyé",
            "ERROR" => "⚠ Erreur",
            "FAILED" => "⚠ Erreur",
            "SIGNING" => "⏳ Signature...",
            "UNSIGNED" => "● En attente",
            "PENDING" => "● En attente",
            _ => "● En attente"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class DocTypeToShortLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s))
            return "doc";

        var upper = s.ToUpperInvariant();
        if (upper.Contains("ATTEST")) return "attestation";
        if (upper.Contains("RELEVE")) return "releve";
        if (upper.Contains("REUSS")) return "reussite";
        if (upper.Contains("INSCRIP")) return "inscription";
        return "doc";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
