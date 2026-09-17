using TimeTracker.Domain.Abstractions;

namespace TimeTracker.Domain.TimeTracking.Events;

/// <summary>
/// Доменное событие: запущена новая запись времени
/// </summary>
public sealed record TimerStartedEvent(
    /// <summary>
    /// Id записи, к которой относится событие
    /// </summary>
    TimeEntryId EntryId,

    /// <summary>
    /// Момент, когда запись стартовала.
    /// </summary>
    DateTimeOffset StartedAt) : IDomainEvent
{
    /// <summary>
    /// Реализация IDomainEvent. Для этого события момент возникновения
    /// совпадает со стартом записи.
    /// </summary>
    public DateTimeOffset OccurredAt => StartedAt;
}
