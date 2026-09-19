using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TimeTracker.Application.TimeTracking;

namespace TimeTracker.Wpf.Converters;

/// <summary>
/// Показывает элемент, если TimerState равен параметру конвертера.
///
/// Использование в XAML:
///   Visibility="{Binding State, Converter={StaticResource TimerStateToVisibility},
///                ConverterParameter=Running}"
///
/// Применяется к кнопкам Start/Pause/Resume/Stop, чтобы показывать
/// только актуальные действия в текущем состоянии.
/// </summary>
public sealed class TimerStateToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimerState state || parameter is not string expected)
            return Visibility.Collapsed;

        return Enum.TryParse<TimerState>(expected, out var expectedState) && state == expectedState
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
