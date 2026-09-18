namespace TimeTracker.Application.TimeTracking;

/// <summary>
/// Состояние таймера.
///
/// Переходы:
///   Idle    в Running   : StartAsync
///   Running в Paused    : Pause
///   Paused  в Running   : Resume
///   Running в Idle      : StopAsync
///   Paused  в Idle      : StopAsync
///
/// Из Idle нельзя Pause/Resume — нет активной записи.
/// Из Running нельзя StartAsync — уже идёт запись.
/// </summary>
public enum TimerState
{
    /// <summary>Таймер не запущен. Активной записи нет</summary>
    Idle,

    /// <summary>Таймер идет. Активная запись есть, время считается</summary>
    Running,

    /// <summary>
    /// Таймер на паузе. Активная запись есть, но время не считается.
    /// Длительность паузы накапливается отдельно и не попадает
    /// в Duration записи
    /// </summary>
    Paused
}
