using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: управление записью времени и чтение ее длительности.
/// </summary>
public interface ITimerControl
{
    /// <summary>
    /// Признак того, что запись идет.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Признак того, что запись приостановлена.
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// Признак того, что запись завершена и зафиксирована.
    /// </summary>
    bool IsFinished { get; }

    /// <summary>
    /// Запускает запись с указанным проектом.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — запись без проекта.</param>
    Task Start(Guid? projectId);

    /// <summary>
    /// Приостанавливает идущую запись.
    /// </summary>
    Task Pause();

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    Task Resume();

    /// <summary>
    /// Завершает запись и сохраняет ее.
    /// </summary>
    Task Stop();

    /// <summary>
    /// Возвращает длительность записи без времени пауз.
    /// </summary>
    Duration GetElapsed();

    /// <summary>
    /// Восстанавливает незавершенную сессию из хранилища.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RestoreAsync(CancellationToken cancellationToken = default);
}
