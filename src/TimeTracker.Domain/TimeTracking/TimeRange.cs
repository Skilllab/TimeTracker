using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.TimeTracking;

/// <summary>
/// Диапазон времени в течение которого шла запись.
///
/// Хранит момент старта и момент окончания. End == null означает, что запись идет прямо сейчас
/// </summary>
public readonly record struct TimeRange
{
    /// <summary>
    /// Момент старта записи. Всегда задан.
    /// Хранится в UTC — локальное время только для UI.
    /// </summary>
    public DateTimeOffset Start
    {
        get;
    }

    /// <summary>
    /// Момент окончания записи. null — запись идет прямо сейчас.
    ///
    /// Заполняется один раз при вызове TimeEntry.Stop() и после этого
    /// не меняется. Повторный Stop запрещен
    /// </summary>
    public DateTimeOffset? End
    {
        get;
    }

    /// <summary>
    /// Создать диапазон
    /// </summary>
    /// <param name="start">Момент старта записи</param>
    /// <param name="end">Момент окончания записи</param>
    public TimeRange(DateTimeOffset start, DateTimeOffset? end)
    {
        if (end.HasValue && end.Value < start)
            throw new DomainException("End must be after Start.");

        Start = start;
        End = end;
    }

    /// <summary>
    /// Запись идет прямо сейчас
    /// </summary>
    public bool IsRunning => End is null;

    /// <summary>
    /// Длительность записи на текущий момент.
    ///
    /// Для завершенной записи (End != null) — фиксированное значение,
    /// не меняется со временем.
    /// Для running-записи (End == null) — время от Start до текущего
    /// момента, вычисляется при каждом обращении.
    /// </summary>
    public Duration Duration => End is null
        ? Duration.FromTimeSpan(DateTimeOffset.UtcNow - Start)
        : Duration.FromTimeSpan(End.Value - Start);

    /// <summary>
    /// Длительность на конкретный момент времени
    /// </summary>
    /// <param name="moment">Момент времени, для которого нужно рассчитать длительность</param>
    public Duration DurationAt(DateTimeOffset moment)
    {
        if (moment < Start)
            throw new DomainException("Moment cannot be before Start.");

        return End is null
            ? Duration.FromTimeSpan(moment - Start)
            : Duration.FromTimeSpan(End.Value - Start);
    }
}
