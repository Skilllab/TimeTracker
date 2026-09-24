using TimeTracker.Application;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта управления проектами для дизайнера XAML: ничего не меняет.
/// </summary>
internal sealed class DesignProjectEditor : IProjectEditor
{
    /// <summary>
    /// Создает проект.
    /// </summary>
    /// <param name="name">Имя проекта.</param>
    /// <param name="color">Цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task CreateAsync(string name, string color, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Переименовывает проект.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="name">Новое имя проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task RenameAsync(Guid projectId, string name, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Меняет цвет маркера проекта.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="color">Новый цвет маркера в виде #RRGGBB из набора палитры.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task ChangeColorAsync(Guid projectId, string color, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Архивирует проект.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task ArchiveAsync(Guid projectId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Возвращает проект из архива.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task UnarchiveAsync(Guid projectId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
