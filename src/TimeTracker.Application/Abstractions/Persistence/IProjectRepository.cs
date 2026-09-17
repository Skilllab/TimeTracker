using TimeTracker.Domain.Projects;

namespace TimeTracker.Application.Abstractions.Persistence;

/// <summary>
/// Репозиторий проектов
/// </summary>
public interface IProjectRepository
{
    /// <summary>Найти проект по Id. Возвращает null, если проекта нет</summary>
    /// <param name="id">Идентификатор проекта</param>
    /// <param name="ct">Токен отмены операции</param>
    Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct = default);

    /// <summary>
    /// Все проекты, включая архивные. Сортировка по имени.
    /// Нужно для отчётов и редактирования старых записей.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Только активные (не архивные) проекты. Сортировка по имени.
    /// Используется в UI для выбора проекта при старте таймера.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    Task<IReadOnlyList<Project>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Добавить проект в ChangeTracker. Не сохраняет в БД.
    /// </summary>
    /// <param name="project">Проект для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task AddAsync(Project project, CancellationToken ct = default);

    /// <summary>Пометить проект на удаление в ChangeTracker.</summary>
    /// <param name="project">Проект для удаления.</param>
    void Remove(Project project);
}
