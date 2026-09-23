namespace TimeTracker.Application;

/// <summary>
/// Сценарий реакции на простой: приостанавливает идущую запись, когда активность отсутствует дольше порога.
/// </summary>
public sealed class IdleWatcher : IDisposable
{
    private readonly IIdleDetector _detector;
    private readonly ITimerControl _timerControl;
    private readonly IdleSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _pollInterval;
    private ITimer? _timer;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="detector">Исходящий порт определения простоя.</param>
    /// <param name="timerControl">Входящий порт управления записью.</param>
    /// <param name="settings">Настройка порога простоя.</param>
    /// <param name="timeProvider">Источник времени.</param>
    /// <param name="pollInterval">Период опроса простоя.</param>
    public IdleWatcher(
        IIdleDetector detector,
        ITimerControl timerControl,
        IdleSettings settings,
        TimeProvider timeProvider,
        TimeSpan pollInterval)
    {
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _timerControl = timerControl ?? throw new ArgumentNullException(nameof(timerControl));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _pollInterval = pollInterval;
    }

    /// <summary>
    /// Начинает наблюдение за активностью.
    /// </summary>
    public void Start()
    {
        _timer ??= _timeProvider.CreateTimer(_ => Check(), null, _pollInterval, _pollInterval);
    }

    /// <summary>
    /// Прекращает наблюдение за активностью.
    /// </summary>
    public void Dispose() => _timer?.Dispose();

    /// <summary>
    /// Проверяет простой и приостанавливает запись, если порог превышен.
    /// </summary>
    public void Check()
    {
        if (_detector.GetIdleTime() < _settings.Threshold)
        {
            return;
        }

        if (!_timerControl.IsRunning)
        {
            return;
        }

        _ = _timerControl.Pause();
    }
}
