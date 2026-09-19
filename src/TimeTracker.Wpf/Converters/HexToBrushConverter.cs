using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeTracker.Wpf.Converters;

/// <summary>
/// Конвертирует hex-строку "#RRGGBB" в SolidColorBrush для UI.
///
/// Используется для маркера проекта в списке записей.
/// При невалидном hex возвращает серый — UI не падает.
/// </summary>
public sealed class HexToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Fallback =
        new(Color.FromRgb(0x99, 0x99, 0x99));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            return Fallback;

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
        catch (FormatException)
        {
            return Fallback;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
