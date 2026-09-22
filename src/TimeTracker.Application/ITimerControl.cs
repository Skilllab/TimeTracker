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
    /// Запускает запись.
    /// </summary>
    void Start();

    /// <summary>
    /// Приостанавливает идущую запись.
    /// </summary>
    void Pause();

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    void Resume();

    /// <summary>
    /// Завершает запись и сохраняет ее.
    /// </summary>
    Task Stop();

    /// <summary>
    /// Возвращает длительность записи без времени пауз.
    /// </summary>
    Duration GetElapsed();
}
