namespace TimeTracker.Application.TimeTracking;

/// <summary>
/// Аргументы события StateChanged. Поднимается при каждом переходе
/// стейт-машины: Idle в Running, Running в Paused, Paused в Running,
/// Running в Idle, Paused в Idle.
///
/// UI подписывается на это событие, чтобы переключить кнопки
/// Start/Pause/Resume/Stop и обновить индикатор.
/// </summary>
public sealed class TimerStateChangedEventArgs(TimerState oldState, TimerState newState) : EventArgs
{
    /// <summary>Состояние до перехода</summary>
    public TimerState OldState { get; } = oldState;

    /// <summary>Состояние после перехода</summary>
    public TimerState NewState { get; } = newState;
}
