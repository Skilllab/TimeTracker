using System;
using System.Threading.Tasks;
using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий управления записью времени: читает часы, делегирует в доменную сессию и сохраняет результат.
/// </summary>
public sealed class TimerControl : ITimerControl
{
    private readonly TimerSession _session;
    private readonly TimeProvider _timeProvider;
    private readonly ITimeEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="session">Сессия таймера.</param>
    /// <param name="timeProvider">Источник времени.</param>
    /// <param name="repository">Исходящий порт хранилища записей.</param>
    /// <param name="unitOfWork">Исходящий порт фиксации изменений.</param>
    public TimerControl(
        TimerSession session,
        TimeProvider timeProvider,
        ITimeEntryRepository repository,
        IUnitOfWork unitOfWork)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
    /// Признак того, что запись завершена и сохранена.
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
    /// Возвращает длительность записи без времени пауз.
    /// </summary>
    public Duration GetElapsed() => _session.ElapsedAt(_timeProvider.GetUtcNow());

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    public void Resume() => _session.Resume(_timeProvider.GetUtcNow());

    /// <summary>
    /// Завершает запись и сохраняет ее.
    /// </summary>
    public async Task Stop()
    {
        _session.Stop(_timeProvider.GetUtcNow());

        var current = _session.Current!;
        var entry = new TimeEntry(
            Guid.NewGuid(),
            string.Empty,
            current.Start,
            current.End,
            _session.PausedSeconds,
            false,
            null);

        await _repository.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();
    }
}
