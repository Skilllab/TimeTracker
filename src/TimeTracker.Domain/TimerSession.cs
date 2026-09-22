namespace TimeTracker.Domain;

/// <summary>
/// Сессия таймера: хранит интервал записи и паузы и следит за состоянием.
/// </summary>
public sealed class TimerSession
{
    private readonly List<TimeRange> _pauses = new();
    private TimeRange? _current;
    private DateTimeOffset? _pausedAt;

    /// <summary>
    /// Текущее состояние записи.
    /// </summary>
    public TimerState State { get; private set; } = TimerState.Idle;

    /// <summary>
    /// Текущий интервал записи; <c>null</c>, пока запись не начата.
    /// </summary>
    public TimeRange? Current => _current;

    /// <summary>
    /// Начинает запись с указанного момента.
    /// </summary>
    /// <param name="now">Момент начала записи.</param>
    public void Start(DateTimeOffset now)
    {
        if (State != TimerState.Idle)
        {
            throw new InvalidTimerStateException("Нельзя начать запись, если она уже начата или завершена.");
        }

        _current = new TimeRange(now);
        State = TimerState.Running;
    }

    /// <summary>
    /// Приостанавливает идущую запись.
    /// </summary>
    /// <param name="now">Момент приостановки.</param>
    public void Pause(DateTimeOffset now)
    {
        if (State != TimerState.Running)
        {
            throw new InvalidTimerStateException("Нельзя приостановить запись, которая не идет.");
        }

        _pausedAt = now;
        State = TimerState.Paused;
    }

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    /// <param name="now">Момент возобновления.</param>
    public void Resume(DateTimeOffset now)
    {
        if (State != TimerState.Paused || _pausedAt is null)
        {
            throw new InvalidTimerStateException("Нельзя возобновить запись, которая не приостановлена.");
        }

        _pauses.Add(new TimeRange(_pausedAt.Value, now));
        _pausedAt = null;
        State = TimerState.Running;
    }

    /// <summary>
    /// Завершает запись и фиксирует ее длительность.
    /// </summary>
    /// <param name="now">Момент завершения.</param>
    public void Stop(DateTimeOffset now)
    {
        if (State != TimerState.Running && State != TimerState.Paused)
        {
            throw new InvalidTimerStateException("Нельзя завершить незапущенную запись.");
        }

        if (_pausedAt is not null)
        {
            _pauses.Add(new TimeRange(_pausedAt.Value, now));
            _pausedAt = null;
        }

        _current = new TimeRange(_current!.Start, now);
        State = TimerState.Finished;
    }

    /// <summary>
    /// Считает длительность записи без времени пауз.
    /// </summary>
    /// <param name="now">Текущий момент времени.</param>
    public Duration ElapsedAt(DateTimeOffset now)
    {
        if (_current is null)
        {
            return Duration.Zero;
        }

        var end = State switch
        {
            TimerState.Paused => _pausedAt!.Value,
            TimerState.Finished => _current.End!.Value,
            _ => now
        };

        var total = end <= _current.Start ? TimeSpan.Zero : end - _current.Start;

        foreach (var pause in _pauses)
        {
            total -= pause.End!.Value - pause.Start;
        }

        return total <= TimeSpan.Zero ? Duration.Zero : Duration.From(total);
    }
}
