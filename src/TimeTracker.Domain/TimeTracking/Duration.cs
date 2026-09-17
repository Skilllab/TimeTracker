using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.TimeTracking;

/// <summary>
/// Длительность записи времени.
///
/// Хранит, сколько времени заняла одна TimeEntry. Используется везде,
/// где нужно передать или отобразить «промежуток времени, который уже прошел»
/// </summary>
public readonly record struct Duration
{
    /// <summary>
    /// Значение промежутка
    /// </summary>
    public TimeSpan Value
    {
        get;
    }


    private Duration(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
            throw new DomainException("Duration cannot be negative.");

        Value = value;
    }

    /// <summary>
    /// Нулевая длительность. Единственный Duration, который можно
    /// получить без явного указания значения
    /// </summary>
    public static Duration Zero => new(TimeSpan.Zero);

    /// <summary>
    /// Создать Duration из TimeSpan. Основная фабрика.
    /// </summary>
    public static Duration FromTimeSpan(TimeSpan value) => new(value);

    /// <summary>
    /// Создать Duration из количества секунд
    /// </summary>
    public static Duration FromSeconds(long seconds)
        => new(TimeSpan.FromSeconds(seconds));

    /// <summary>
    /// Сложить две длительности. Результат — новый Duration,
    /// исходные не меняются (immutable).
    /// Используется в отчетах для суммирования длительностей записей и в TimerService для накопления пауз.
    /// </summary>
    /// <param name="other">Длительность, которую нужно добавить</param>
    public Duration Add(Duration other) => new(Value + other.Value);

    /// <summary>
    /// Форматирование для UI:
    ///   - до часа — "MM:SS" (например, "05:30"),
    ///   - от часа — "HH:MM:SS" (например, "01:30:45").
    ///
    /// Используется для отображения текущего таймера и в отчетах.
    /// </summary>
    public string ToHumanReadable()
    {
        var ts = Value;
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}"
            : $"{ts.Minutes:00}:{ts.Seconds:00}";
    }
}
