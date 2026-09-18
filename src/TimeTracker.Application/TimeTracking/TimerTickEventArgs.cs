using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Application.TimeTracking;

/// <summary>
/// Аргументы события Tick. Поднимается раз в секунду, пока таймер
/// находится в состоянии Running.
///
/// Duration — текущая длительность сессии БЕЗ учета пауз.
/// Считается от Range.Start активной записи минус накопленные паузы.
/// </summary>
public sealed class TimerTickEventArgs(Duration duration) : EventArgs
{
    /// <summary>Текущая длительность сессии без пауз</summary>
    public Duration Duration { get; } = duration;
}
