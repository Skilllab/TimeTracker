namespace TimeTracker.Application;

/// <summary>
/// Сценарий реакции на простой: приостанавливает идущую запись, когда активность отсутствует дольше порога.
/// </summary>
public sealed class IdleWatcher : IDisposable
{
    private readonly IIdleDetector _detector;
    private readonly ITaskControl _taskControl;
    private readonly IdleSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _pollInterval;
    private ITimer? _timer;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="detector">Исходящий порт определения простоя.</param>
    /// <param name="taskControl">Входящий порт управления задачами.</param>
    /// <param name="settings">Настройка порога простоя.</param>
    /// <param name="timeProvider">Источник времени.</param>
    /// <param name="pollInterval">Период опроса простоя.</param>
    public IdleWatcher(
        IIdleDetector detector,
        ITaskControl taskControl,
        IdleSettings settings,
        TimeProvider timeProvider,
        TimeSpan pollInterval)
    {
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _taskControl = taskControl ?? throw new ArgumentNullException(nameof(taskControl));
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
    /// Проверяет простой и приостанавливает идущую задачу, если порог превышен.
    /// Задача выбирается сценарием: наблюдение за простоем не знает состояние списка,
    /// а отсутствие идущей задачи для сценария означает пустую операцию.
    /// </summary>
    public void Check()
    {
        if (_detector.GetIdleTime() < _settings.Threshold)
        {
            return;
        }

        _ = _taskControl.PauseRunningAsync();
    }
}
