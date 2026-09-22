namespace TimeTracker.Domain;

/// <summary>
/// Сессия таймера: хранит интервал записи и паузы и следит за состоянием.
/// </summary>
public sealed class TimerSession
{
    private readonly List<TimeRange> _pauses = new();
    private TimeRange? _current;
    private DateTimeOffset? _pausedAt;
    private TimeSpan _pausedTotal;

    /// <summary>
    /// Накопленное время пауз в секундах.
    /// </summary>
    public int PausedSeconds => (int)_pausedTotal.TotalSeconds;

    /// <summary>
    /// Момент начала текущей паузы; <c>null</c>, если паузы нет.
    /// </summary>
    public DateTimeOffset? PausedAt => _pausedAt;


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
    /// Создает открытый интервал записи, поэтому конца у него нет,
    /// и переводит сессию в состояние записи.
    /// Начать можно только из свободного состояния: если запись уже идет
    /// или завершена, бросается <c>InvalidTimerStateException</c> и состояние не меняется.
    /// Накопленное время пауз на момент начала равно нулю.
    /// </summary>
    /// <param name="now">Момент начала записи; он же начало интервала.</param>
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
    /// Запоминает момент начала паузы, но не пополняет накопленное время пауз:
    /// незакрытая пауза начинает учитываться только при возобновлении или завершении,
    /// поэтому на время паузы длительность записи замирает.
    /// Приостановить можно только идущую запись: в остальных состояниях
    /// бросается <c>InvalidTimerStateException</c> и состояние не меняется.
    /// </summary>
    /// <param name="now">Момент приостановки; он же момент начала паузы.</param>
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
    /// Приостанавливает идущую запись.
    /// Запоминает момент начала паузы, но не пополняет накопленное время пауз:
    /// незакрытая пауза начинает учитываться только при возобновлении или завершении,
    /// поэтому на время паузы длительность записи замирает.
    /// Приостановить можно только идущую запись: в остальных состояниях
    /// бросается <c>InvalidTimerStateException</c> и состояние не меняется.
    /// </summary>
    /// <param name="now">Момент приостановки; он же момент начала паузы.</param>
    public void Resume(DateTimeOffset now)
    {
        if (State != TimerState.Paused || _pausedAt is null)
        {
            throw new InvalidTimerStateException("Нельзя возобновить запись, которая не приостановлена.");
        }

        if (now < _pausedAt.Value)
        {
            throw new InvalidTimerStateException("Момент возобновления не может быть раньше начала паузы.");
        }

        _pausedTotal += now - _pausedAt.Value;
        _pausedAt = null;
        State = TimerState.Running;
    }

    /// <summary>
    /// Завершает запись и фиксирует ее длительность.
    /// Если запись стоит на паузе, незакрытая пауза пополняет накопленное время пауз,
    /// поэтому простой до момента завершения в длительность не попадает.
    /// Интервал записи закрывается моментом завершения, а состояние становится конечным:
    /// после завершения ни начать, ни приостановить запись нельзя.
    /// Завершить можно только идущую или приостановленную запись, иначе
    /// бросается <c>InvalidTimerStateException</c> и состояние не меняется.
    /// </summary>
    /// <param name="now">Момент завершения; он же конец интервала записи.</param>
    public void Stop(DateTimeOffset now)
    {
        if (State != TimerState.Running && State != TimerState.Paused)
        {
            throw new InvalidTimerStateException("Нельзя завершить незапущенную запись.");
        }

        if (_pausedAt is not null)
        {
            _pausedTotal += now - _pausedAt.Value;
            _pausedAt = null;
        }

        _current = new TimeRange(_current!.Start, now);
        State = TimerState.Finished;
    }

    /// <summary>
    /// Завершает запись и фиксирует ее длительность.
    /// Если запись стоит на паузе, незакрытая пауза пополняет накопленное время пауз,
    /// поэтому простой до момента завершения в длительность не попадает.
    /// Интервал записи закрывается моментом завершения, а состояние становится конечным:
    /// после завершения ни начать, ни приостановить запись нельзя.
    /// Завершить можно только идущую или приостановленную запись, иначе
    /// бросается <c>InvalidTimerStateException</c> и состояние не меняется.
    /// </summary>
    /// <param name="now">Момент завершения; он же конец интервала записи.</param>
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

        var total = end <= _current.Start ? TimeSpan.Zero : end - _current.Start - _pausedTotal;

        return total <= TimeSpan.Zero ? Duration.Zero : Duration.From(total);
    }

    /// <summary>
    /// Завершает запись и фиксирует ее длительность.
    /// Если запись стоит на паузе, незакрытая пауза пополняет накопленное время пауз,
    /// поэтому простой до момента завершения в длительность не попадает.
    /// Интервал записи закрывается моментом завершения, а состояние становится конечным:
    /// после завершения ни начать, ни приостановить запись нельзя.
    /// Завершить можно только идущую или приостановленную запись, иначе
    /// бросается <c>InvalidTimerStateException</c> и состояние не меняется.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="now">Момент завершения; он же конец интервала записи.</param>
    public void Restore(TimeEntry entry, DateTimeOffset now)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (!entry.IsOpen)
        {
            throw new InvalidTimerStateException("Восстановить можно только незавершенную запись.");
        }

        _current = new TimeRange(entry.StartedAt);
        _pausedTotal = TimeSpan.FromSeconds(entry.PausedSeconds);
        _pausedAt = entry.PausedAt ?? now;
        State = TimerState.Paused;
    }
}
