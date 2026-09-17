namespace TimeTracker.Domain.Abstractions;

/// <summary>
/// Маркер доменного события
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Время события
    /// </summary>
    DateTimeOffset OccurredAt
    {
        get;
    }
}
