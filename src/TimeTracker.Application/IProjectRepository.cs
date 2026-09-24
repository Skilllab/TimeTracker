using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: хранилище проектов.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Возвращает все проекты в порядке хранения, включая архивные.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает проект по идентификатору; <c>null</c>, если проекта нет.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет проект.
    /// </summary>
    /// <param name="project">Добавляемый проект.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет проект.
    /// </summary>
    /// <param name="project">Обновляемый проект.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}
