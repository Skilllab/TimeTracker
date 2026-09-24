namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: управление проектами.
/// </summary>
public interface IProjectEditor
{
    /// <summary>
    /// Создает проект.
    /// </summary>
    /// <param name="name">Имя проекта.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task CreateAsync(string name, string color, CancellationToken cancellationToken = default);

    /// <summary>
    /// Переименовывает проект.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="name">Новое имя проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RenameAsync(Guid projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Меняет цвет маркера проекта.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="color">Новый цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ChangeColorAsync(Guid projectId, string color, CancellationToken cancellationToken = default);

    /// <summary>
    /// Архивирует проект.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ArchiveAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает проект из архива.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task UnarchiveAsync(Guid projectId, CancellationToken cancellationToken = default);
}
