namespace TimeTracker.Domain;

/// <summary>
/// Сессия таймера: хранит текущий интервал и следит за его состоянием.
/// </summary>
public sealed class TimerSession
{
    private TimeRange? _current;

    /// <summary>
    /// Признак того, что запись идет.
    /// </summary>
    public bool IsRunning => _current?.IsOpen ?? false;

    /// <summary>
    /// Текущий интервал: открытый, пока запись идет, и закрытый после остановки.
    /// </summary>
    public TimeRange? Current => _current;

    /// <summary>
    /// Начинает новую запись с указанного момента.
    /// </summary>
    /// <param name="now">Момент начала записи.</param>
    public void Start(DateTimeOffset now) => _current = new TimeRange(now);

    /// <summary>
    /// Останавливает активную запись.
    /// </summary>
    /// <param name="now">Момент остановки.</param>
    public void Stop(DateTimeOffset now)
    {
        if (_current is null || !_current.IsOpen)
        {
            throw new InvalidOperationException("Нельзя остановить незапущенную запись.");
        }

        _current = new TimeRange(_current.Start, now);
    }
}
