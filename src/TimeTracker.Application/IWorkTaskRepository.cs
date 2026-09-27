using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: хранилище задач.
/// </summary>
public interface IWorkTaskRepository
{
    /// <summary>
    /// Добавляет задачу.
    /// </summary>
    /// <param name="task">Добавляемая задача.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task AddAsync(WorkTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает все задачи независимо от состояния.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<WorkTask>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает задачу по идентификатору; <c>null</c>, если задачи нет.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает выполняемую задачу; <c>null</c>, если ни одна задача не выполняется.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<WorkTask?> GetRunningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет задачу.
    /// </summary>
    /// <param name="task">Обновляемая задача.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает задачи, созданные в указанном интервале, в порядке создания.
    /// </summary>
    /// <param name="from">Начало интервала выборки.</param>
    /// <param name="to">Конец интервала выборки.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<WorkTask>> GetRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}
