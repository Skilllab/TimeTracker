using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: список записей времени за сегодня.
/// </summary>
public interface ITimeEntryList
{
    /// <summary>
    /// Возвращает записи за сегодня в порядке начала.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<TimeEntry>> GetTodayAsync(CancellationToken cancellationToken = default);
}
