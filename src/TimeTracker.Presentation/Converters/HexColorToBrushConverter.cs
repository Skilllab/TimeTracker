using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TimeTracker.Presentation.Converters;

/// <summary>
/// Преобразует строку цвета вида #RRGGBB в кисть маркера проекта.
/// Пустая строка и неизвестный формат дают прозрачную кисть,
/// поэтому у записи без проекта маркер просто не виден.
/// </summary>
public sealed class HexColorToBrushConverter : IValueConverter
{
    /// <summary>
    /// Преобразует строку цвета в кисть.
    /// </summary>
    /// <param name="value">Строка цвета вида #RRGGBB.</param>
    /// <param name="targetType">Тип результата привязки.</param>
    /// <param name="parameter">Параметр преобразования; не используется.</param>
    /// <param name="culture">Культура преобразования.</param>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && Color.TryParse(text, out var color))
        {
            return new SolidColorBrush(color);
        }

        return Brushes.Transparent;
    }

    /// <summary>
    /// Обратное преобразование не поддерживается: кисть не превращается обратно в строку.
    /// </summary>
    /// <param name="value">Значение привязки.</param>
    /// <param name="targetType">Тип результата.</param>
    /// <param name="parameter">Параметр преобразования.</param>
    /// <param name="culture">Культура преобразования.</param>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
