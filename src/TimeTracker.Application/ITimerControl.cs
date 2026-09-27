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
    /// Запускает запись с указанным именем задачи и проектом.
    /// </summary>
    /// <param name="name">Имя задачи; обязательно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — запись без проекта.</param>
    Task Start(string name, Guid? projectId);

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
    /// Переводит таймер в исходное состояние для следующей задачи.
    /// Завершенная запись при этом не меняется.
    /// </summary>
    Task NewTask();

    /// <summary>
    /// Переименовывает идущую или приостановленную запись.
    /// </summary>
    /// <param name="name">Новое имя задачи; обязательно.</param>
    Task Rename(string name);

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
