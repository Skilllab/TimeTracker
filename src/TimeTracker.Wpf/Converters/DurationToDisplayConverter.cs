using System.Globalization;
using System.Windows.Data;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Wpf.Converters;

/// <summary>
/// Форматирует Duration в строку для UI: MM:SS или HH:MM:SS.
///
/// Делегирует в Duration.ToHumanReadable() — там логика формата.
/// Один экземпляр может использоваться для OneWay-привязок.
/// </summary>
public sealed class DurationToDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Duration duration
            ? duration.ToHumanReadable()
            : "00:00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
