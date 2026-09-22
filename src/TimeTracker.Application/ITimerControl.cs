using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: управление активной записью времени.
/// </summary>
public interface ITimerControl
{
    /// <summary>
    /// Признак того, что запись идет.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Начинает новую запись.
    /// </summary>
    void Start();

    /// <summary>
    /// Останавливает активную запись.
    /// </summary>
    void Stop();

    /// <summary>
    /// Возвращает длительность текущей записи к текущему моменту.
    /// </summary>
    Duration GetElapsed();
}
