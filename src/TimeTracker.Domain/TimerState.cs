namespace TimeTracker.Domain;

/// <summary>
/// Состояние записи времени.
/// </summary>
public enum TimerState
{
    /// <summary>Запись не начата.</summary>
    Idle,

    /// <summary>Запись идет.</summary>
    Running,

    /// <summary>Запись приостановлена.</summary>
    Paused,

    /// <summary>Запись завершена и зафиксирована.</summary>
    Finished
}
