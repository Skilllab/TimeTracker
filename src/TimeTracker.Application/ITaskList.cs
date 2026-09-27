namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: список задач для показа.
/// </summary>
public interface ITaskList
{
    /// <summary>
    /// Возвращает задачи, упорядоченные для показа: выполняемая сверху, затем недавно
    /// запускавшиеся, а завершенные всегда в конце.
    /// Поиск и отбор по проекту только сужают список и не меняют состояние задач.
    /// </summary>
    /// <param name="search">Строка минипоиска по наименованию; <c>null</c> или пустая строка — без фильтра.</param>
    /// <param name="tagSearch">Строка минипоиска по тегам; <c>null</c> или пустая строка — без фильтра.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<TaskListItem>> GetAsync(string? search, string? tagSearch, Guid? projectId, CancellationToken cancellationToken = default);
}
