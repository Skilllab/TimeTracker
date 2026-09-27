namespace TimeTracker.Domain;

/// <summary>
/// Длительность как значение. По инварианту не может быть отрицательной.
/// </summary>
public readonly record struct Duration
{
    /// <summary>Создает длительность из уже проверенного значения.</summary>
    private Duration(TimeSpan value) => Value = value;

    /// <summary>Внутреннее представление длительности.</summary>
    public TimeSpan Value { get; }

    /// <summary>Нулевая длительность.</summary>
    public static Duration Zero { get; } = new(TimeSpan.Zero);

    /// <summary>
    /// Создает длительность из интервала времени.
    /// </summary>
    /// <param name="value">Интервал времени; допускается только неотрицательный.</param>
    public static Duration From(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Длительность не может быть отрицательной.");
        }

        return new Duration(value);
    }

    /// <summary>
    /// Форматирует длительность как часы, минуты и секунды; доли секунды усекаются.
    /// Каждый разряд выводится двумя цифрами, часы не переносятся в сутки.
    /// </summary>
    public string ToClockString()
    {
        var totalSeconds = (long)Value.TotalSeconds;
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds % 3600 / 60;
        var seconds = totalSeconds % 60;

        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Представить длительность как строку.
    /// </summary>
    public override string ToString() => ToClockString();
}
