using System;
using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий управления записью времени: читает часы и делегирует в доменную сессию.
/// </summary>
public sealed class TimerControl : ITimerControl
{
    private readonly TimerSession _session;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="session">Сессия таймера.</param>
    /// <param name="timeProvider">Источник времени.</param>
    public TimerControl(TimerSession session, TimeProvider timeProvider)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Признак того, что запись идет.
    /// </summary>
    public bool IsRunning => _session.State == TimerState.Running;

    /// <summary>
    /// Признак того, что запись приостановлена.
    /// </summary>
    public bool IsPaused => _session.State == TimerState.Paused;

    /// <summary>
    /// Признак того, что запись завершена и зафиксирована.
    /// </summary>
    public bool IsFinished => _session.State == TimerState.Finished;

    /// <summary>
    /// Запускает запись.
    /// </summary>
    public void Start() => _session.Start(_timeProvider.GetUtcNow());

    /// <summary>
    /// Приостанавливает идущую запись.
    /// </summary>
    public void Pause() => _session.Pause(_timeProvider.GetUtcNow());

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    public void Resume() => _session.Resume(_timeProvider.GetUtcNow());

    /// <summary>
    /// Завершает запись.
    /// </summary>
    public void Stop() => _session.Stop(_timeProvider.GetUtcNow());

    /// <summary>
    /// Возвращает длительность записи без времени пауз.
    /// </summary>
    public Duration GetElapsed() => _session.ElapsedAt(_timeProvider.GetUtcNow());
}
