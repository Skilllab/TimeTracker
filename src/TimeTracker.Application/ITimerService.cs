using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: источник активного интервала времени.
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Возвращает длительность активной записи от ее начала до текущего момента.
    /// </summary>
    Duration GetElapsed();
}
