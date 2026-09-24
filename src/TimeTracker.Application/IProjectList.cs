using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: список проектов для показа и выбора.
/// </summary>
public interface IProjectList
{
    /// <summary>
    /// Возвращает проекты в порядке, пригодном для показа.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAvailableAsync(CancellationToken cancellationToken = default);
}
