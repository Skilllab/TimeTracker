using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: хранилище записей времени.
/// </summary>
public interface ITimeEntryRepository
{
    /// <summary>
    /// Добавляет запись времени.
    /// </summary>
    /// <param name="entry">Добавляемая запись.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает идущую запись; <c>null</c>, если незавершенных записей нет.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает записи, начавшиеся в указанном интервале, в порядке начала.
    /// </summary>
    /// <param name="from">Начало интервала выборки.</param>
    /// <param name="to">Конец интервала выборки.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<TimeEntry>> GetRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
