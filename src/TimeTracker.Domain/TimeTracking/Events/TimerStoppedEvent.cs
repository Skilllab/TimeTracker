using TimeTracker.Domain.Abstractions;

namespace TimeTracker.Domain.TimeTracking.Events;

/// <summary>
/// Доменное событие: запись времени остановлена
/// </summary>
public sealed record TimerStoppedEvent(
    /// <summary>
    /// Id остановленной записи
    /// </summary>
    TimeEntryId EntryId,

    /// <summary>
    /// Момент остановки
    /// </summary>
    DateTimeOffset StoppedAt,

    /// <summary>
    /// Итоговая длительность записи
    /// </summary>
    Duration TotalDuration) : IDomainEvent
{
    /// <summary>
    /// Реализация IDomainEvent. Момент возникновения — момент остановки.
    /// </summary>
    public DateTimeOffset OccurredAt => StoppedAt;
}
