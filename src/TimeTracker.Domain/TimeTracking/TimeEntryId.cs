using TimeTracker.Domain.Common;

namespace TimeTracker.Domain.TimeTracking;

/// <summary>
/// Идентификатор записи времени
/// </summary>
public readonly record struct TimeEntryId(Guid Value)
{
    /// <summary>
    /// Создать новый уникальный Id
    /// </summary>
    public static TimeEntryId New() => new(Guid.NewGuid());

    /// <summary>
    /// Создать Id из готового Guid
    /// </summary>
    /// <param name="value">Guid для создания Id</param>
    public static TimeEntryId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("TimeEntryId cannot be empty.");
        return new TimeEntryId(value);
    }
}
