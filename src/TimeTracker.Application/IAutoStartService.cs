namespace TimeTracker.Application;

/// <summary>
/// Исходящий порт: запуск приложения вместе с системой.
/// </summary>
public interface IAutoStartService
{
    /// <summary>
    /// Включает автозапуск приложения.
    /// </summary>
    void Enable();

    /// <summary>
    /// Выключает автозапуск приложения.
    /// </summary>
    void Disable();

    /// <summary>
    /// Возвращает признак того, что автозапуск включен.
    /// </summary>
    /// <returns>true, если автозапуск включен; иначе false.</returns>
    bool IsEnabled();
}
