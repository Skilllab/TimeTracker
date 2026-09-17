using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Application.Abstractions.Persistence;

/// <summary>
/// Репозиторий записей времени
/// </summary>
public interface ITimeEntryRepository
{
    /// <summary>
    /// Найти запись по Id. Возвращает null если записи нет
    /// </summary>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<TimeEntry?> GetByIdAsync(TimeEntryId id, CancellationToken ct = default);

    /// <summary>
    /// Найти текущую активную (running) запись. Возвращает null
    /// если таймер не запущен. Если running-записей несколько, то
    /// возвращается самая поздняя по Range.Start.
    /// </summary>
    /// <param name="ct">Токен отмены операции</param>
    Task<TimeEntry?> GetRunningAsync(CancellationToken ct = default);

    /// <summary>
    /// Получить записи, у которых Range.Start попадает в [from, to).
    /// </summary>
    /// <param name="rangeStart">Начало диапазона (включительно), UTC</param>
    /// <param name="rangeEnd">Конец диапазона (не включая), UTC</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<IReadOnlyList<TimeEntry>> GetByDateRangeAsync(
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken ct = default);

    /// <summary>
    /// Добавить запись в ChangeTracker. Не сохраняет в БД —
    /// реальное сохранение при SaveChangesAsync.
    /// </summary>
    /// <param name="entry">Запись для добавления</param>
    /// <param name="ct">Токен отмены операции</param>
    Task AddAsync(TimeEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Пометить запись на удаление в ChangeTracker.
    /// Реальное удаление — при SaveChangesAsync.
    /// </summary>
    /// <param name="entry">Запись для удаления</param>
    void Remove(TimeEntry entry);
}
