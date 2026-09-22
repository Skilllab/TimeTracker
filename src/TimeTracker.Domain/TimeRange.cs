namespace TimeTracker.Domain;

/// <summary>
/// Интервал времени: начало и открытый конец.
/// Пока запись идет — конца нет (<see cref="IsOpen"/> = <c>true</c>).
/// </summary>
public sealed record TimeRange
{
    /// <summary>
    /// Создает интервал.
    /// </summary>
    /// <param name="start">Начало интервала.</param>
    /// <param name="end">Конец интервала; <c>null</c> — интервал еще открыт.</param>
    public TimeRange(DateTimeOffset start, DateTimeOffset? end = null)
    {
        if (end < start)
        {
            throw new ArgumentException("Конец интервала не может быть раньше начала.", nameof(end));
        }

        Start = start;
        End = end;
    }

    /// <summary>Начало интервала.</summary>
    public DateTimeOffset Start { get; }

    /// <summary>Конец интервала; <c>null</c>, пока интервал открыт.</summary>
    public DateTimeOffset? End { get; }

    /// <summary>Признак того, что интервал еще не закрыт.</summary>
    public bool IsOpen => End is null;

    /// <summary>
    /// Считает длительность интервала на указанный момент.
    /// Для открытого интервала конец принимается равным <paramref name="now"/>.
    /// </summary>
    /// <param name="now">Текущий момент времени.</param>
    public Duration ElapsedAt(DateTimeOffset now)
    {
        var end = End ?? now;
        return end <= Start ? Duration.Zero : Duration.From(end - Start);
    }
}
