using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: список проектов и сведения о занятом проекте.
/// </summary>
public interface IProjectList
{
    /// <summary>
    /// Возвращает действующие проекты в порядке, пригодном для выбора.
    /// Архивные проекты в результат не попадают.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает проекты для показа на экране проектов.
    /// </summary>
    /// <param name="includeArchived">Признак того, что архивные проекты тоже нужны.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает идентификатор проекта идущей записи; <c>null</c>, если записи нет или проект не задан.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<Guid?> GetActiveProjectIdAsync(CancellationToken cancellationToken = default);
}
