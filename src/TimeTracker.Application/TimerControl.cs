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
    private TimeEntry? _active;

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
    /// Запускает запись с указанным проектом.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — запись без проекта.</param>
    public async Task Start(Guid? projectId)
    {
        var now = _timeProvider.GetUtcNow();
        _session.Start(now);

        _active = new TimeEntry(
            Guid.NewGuid(),
            string.Empty,
            now,
            null,
            0,
            null,
            false,
            projectId);

        await _repository.AddAsync(_active);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Приостанавливает идущую запись.
    /// </summary>
    public async Task Pause()
    {
        var now = _timeProvider.GetUtcNow();
        _session.Pause(now);

        _active = _active!.Pause(now);

        await _repository.UpdateAsync(_active);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Возобновляет приостановленную запись.
    /// </summary>
    public async Task Resume()
    {
        var now = _timeProvider.GetUtcNow();
        _session.Resume(now);

        _active = _active!.Resume(now);

        await _repository.UpdateAsync(_active);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Завершает запись и сохраняет ее.
    /// </summary>
    public async Task Stop()
    {
        var now = _timeProvider.GetUtcNow();
        _session.Stop(now);

        _active = _active!.Close(now);

        await _repository.UpdateAsync(_active);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Восстанавливает незавершенную сессию из хранилища.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        var active = await _repository.GetActiveAsync(cancellationToken);

        if (active is null)
        {
            return;
        }

        _session.Restore(active, _timeProvider.GetUtcNow());
        _active = active;
    }

    /// <summary>
    /// Возвращает длительность записи без времени пауз.
    /// </summary>
    public Duration GetElapsed() => _session.ElapsedAt(_timeProvider.GetUtcNow());

}
