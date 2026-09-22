using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий управления записью времени: владеет доменной сессией и считает ее длительность.
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
    public bool IsRunning => _session.IsRunning;

    /// <summary>
    /// Начинает новую запись.
    /// </summary>
    public void Start() => _session.Start(_timeProvider.GetUtcNow());

    /// <summary>
    /// Останавливает активную запись.
    /// </summary>
    public void Stop() => _session.Stop(_timeProvider.GetUtcNow());

    /// <summary>
    /// Возвращает длительность текущей записи к текущему моменту.
    /// </summary>
    public Duration GetElapsed()
    {
        var current = _session.Current;
        return current is null ? Duration.Zero : current.ElapsedAt(_timeProvider.GetUtcNow());
    }
}
