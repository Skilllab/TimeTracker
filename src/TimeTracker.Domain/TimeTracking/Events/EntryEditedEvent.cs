using TimeTracker.Domain.Abstractions;

namespace TimeTracker.Domain.TimeTracking.Events;

/// <summary>
/// Доменное событие: запись времени отредактирована
/// </summary>
public sealed record EntryEditedEvent(
    /// <summary>
    /// Id отредактированной записи
    /// </summary>
    TimeEntryId EntryId,

    /// <summary>
    /// Момент редактирования
    /// </summary>
    DateTimeOffset EditedAt) : IDomainEvent
{
    /// <summary>
    /// Реализация IDomainEvent. Момент возникновения — момент правки.
    /// </summary>
    public DateTimeOffset OccurredAt => EditedAt;
}
