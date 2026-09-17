using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.Projects;

/// <summary>
/// Цвет проекта.
///
/// Используется в UI для визуального разделения проектов:
/// цветной маркер рядом с записью в списке, цветной сегмент в круговой
/// диаграмме отчетов, цветной фон карточки проекта.
/// </summary>
public readonly record struct ProjectColor
{
    /// <summary>
    /// Hex-представление цвета в формате "#RRGGBB", всегда в верхнем
    /// регистре. Гарантируется фабриками FromHex и FromRgb.
    /// </summary>
    public string Hex
    {
        get;
    }

    private ProjectColor(string hex) => Hex = hex;

    /// <summary>
    /// Создать цвет из hex-строки вида "#RRGGBB"
    ///
    /// Основной способ создания. Используется при редактировании
    /// проекта, где пользователь вводит hex или выбирает из color picker.
    /// </summary>
    /// <param name="hex">Hex-строка вида "#RRGGBB"</param>
    public static ProjectColor FromHex(string hex)
    {
        Guard.AgainstNullOrWhiteSpace(hex, nameof(hex));

        if (hex.Length != 7 || hex[0] != '#')
            throw new DomainException("Color must be in #RRGGBB format.");

        for (var i = 1; i < 7; i++)
        {
            if (!Uri.IsHexDigit(hex[i]))
                throw new DomainException($"Color contains invalid hex character at position {i}.");
        }

        return new ProjectColor(hex.ToUpperInvariant());
    }

    /// <summary>
    /// Создать цвет из трех байтов R, G, B.
    /// Удобно для тестов и для seed-данных где можно задать цвет программно, без hex-строки
    ///
    /// Форматирует байты в hex: каждое число превращается в две hex-цифры
    /// Пример: (0xAB, 0xCD, 0xEF) → "#ABCDEF".
    /// </summary>
    public static ProjectColor FromRgb(byte r, byte g, byte b)
        => new($"#{r:X2}{g:X2}{b:X2}");
}
