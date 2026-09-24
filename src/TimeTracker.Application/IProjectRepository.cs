using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: хранилище проектов.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Возвращает все проекты в порядке хранения.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);
}
