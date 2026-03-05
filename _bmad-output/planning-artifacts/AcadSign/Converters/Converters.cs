using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AcadSign.Models;

namespace AcadSign.Converters;

// ─── Bool → Visibility ────────────────────────────────────────────────────────

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is Visibility.Visible;
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => value is not Visibility.Visible;
}

// ─── String → Visibility ─────────────────────────────────────────────────────

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type t, object p, CultureInfo c)
        => throw new NotImplementedException();
}

// ─── DocumentStatus → Color ──────────────────────────────────────────────────

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var color = value is DocumentStatus s ? s switch
        {
            DocumentStatus.Signed    => "#10B981",
            DocumentStatus.EmailSent => "#6366F1",
            DocumentStatus.Error     => "#EF4444",
            DocumentStatus.Signing   => "#F59E0B",
            _                        => "#F59E0B"
        } : "#F59E0B";
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class StatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var color = value is DocumentStatus s ? s switch
        {
            DocumentStatus.Signed    => "#10B98118",
            DocumentStatus.EmailSent => "#6366F118",
            DocumentStatus.Error     => "#EF444415",
            DocumentStatus.Signing   => "#F59E0B18",
            _                        => "#F59E0B18"
        } : "#F59E0B18";
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class StatusToLabelConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is DocumentStatus s ? s switch
        {
            DocumentStatus.Signed    => "✓ Signé",
            DocumentStatus.EmailSent => "📧 Email envoyé",
            DocumentStatus.Error     => "⚠ Erreur",
            DocumentStatus.Signing   => "⏳ Signature...",
            DocumentStatus.Generating=> "⚙ Génération...",
            _                        => "● En attente"
        } : "—";

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── DocumentStatus → Left Border Color ──────────────────────────────────────

public class StatusToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var color = value is DocumentStatus s ? s switch
        {
            DocumentStatus.Signed    => "#10B981",
            DocumentStatus.EmailSent => "#6366F1",
            DocumentStatus.Error     => "#EF4444",
            DocumentStatus.Signing   => "#F59E0B",
            _                        => "#F59E0B"
        } : "#F59E0B";
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── Bool → String (view mode label) ─────────────────────────────────────────

public class ViewModeToLabelConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? "Vue: Après signature" : "Vue: Avant signature";

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── Zoom → Scale ────────────────────────────────────────────────────────────

public class ZoomToScaleTransformConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var zoom = value is double d ? d : 1.0;
        return new ScaleTransform(zoom, zoom);
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── Double → Percentage String ──────────────────────────────────────────────

public class DoubleToPercentageConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is double d ? $"{(int)(d * 100)}%" : "100%";

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── Pin Length → IsEnabled ──────────────────────────────────────────────────

public class PinLengthToEnabledConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is string s && s.Length >= 4;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── DocumentType → Short Label ──────────────────────────────────────────────

public class DocTypeToShortLabelConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is DocumentType dt ? dt switch
        {
            DocumentType.AttestationScolarite   => "attestation",
            DocumentType.ReleveNotes            => "releve",
            DocumentType.AttestationReussite    => "reussite",
            DocumentType.AttestationInscription => "inscription",
            _                                   => "doc"
        } : "doc";

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// ─── Null → Visibility ───────────────────────────────────────────────────────

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value == null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class InverseNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value == null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
