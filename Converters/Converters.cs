using System.Collections;
using System.Globalization;
using PharmaFind.Helpers;
using PharmaFind.Models;

namespace PharmaFind.Converters;

/// <summary>Inverse une valeur booléenne.</summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}

/// <summary>Vrai si la valeur n'est pas nulle, pas une chaîne vide et pas une collection vide.</summary>
public class HasValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = value switch
        {
            null => false,
            string s => !string.IsNullOrWhiteSpace(s),
            ICollection c => c.Count > 0,
            int i => i > 0,
            _ => true
        };
        return parameter as string == "invert" ? !result : result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Couleur associée à la fraîcheur d'une mise à jour de stock.</summary>
public class FreshnessToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StockFreshness f ? StatusPalette.ForFreshness(f) : StatusPalette.Muted;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Couleur d'un marqueur ou d'une barre selon le statut de disponibilité.</summary>
public class AvailabilityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is AvailabilityStatus s ? StatusPalette.For(s).Foreground : StatusPalette.Muted;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Couleur « ouvert / fermé ».</summary>
public class OpenToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Color.FromArgb("#15803D") : Color.FromArgb("#B91C1C");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Met en gras les éléments non lus (notifications).</summary>
public class UnreadToFontConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is false ? FontAttributes.Bold : FontAttributes.None;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
