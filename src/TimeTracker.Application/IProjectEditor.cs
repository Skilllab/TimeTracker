namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: управление проектами.
/// Архивации нет: проекты не скрываются из выбора, поэтому операций архива и возврата из архива не существует.
/// </summary>
public interface IProjectEditor
{
    /// <summary>
    /// Создает проект.
    /// </summary>
    /// <param name="name">Имя проекта.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB.</param>
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
    /// <param name="color">Новый цвет маркера в виде #RRGGBB.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ChangeColorAsync(Guid projectId, string color, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет проект без возможности восстановления.
    /// Задачи и записи времени остаются: у них только снимается ссылка на проект.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default);
}
