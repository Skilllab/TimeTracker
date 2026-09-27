using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: список проектов и сведения о занятом проекте.
/// </summary>
public interface IProjectList
{
    /// <summary>
    /// Возвращает проекты в порядке, пригодном для выбора.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает проекты для показа.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает идентификатор проекта идущей записи; <c>null</c>, если записи нет или проект не задан.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<Guid?> GetActiveProjectIdAsync(CancellationToken cancellationToken = default);
}
