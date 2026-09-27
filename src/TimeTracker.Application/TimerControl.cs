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
    private readonly IProjectRepository _projectRepository;
    private TimeEntry? _active;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="session">Сессия таймера.</param>
    /// <param name="timeProvider">Источник времени.</param>
    /// <param name="repository">Исходящий порт хранилища записей.</param>
    /// <param name="unitOfWork">Исходящий порт фиксации изменений.</param>
    /// <param name="projectRepository">Исходящий порт хранилища проектов.</param>
    public TimerControl(
        TimerSession session,
        TimeProvider timeProvider,
        ITimeEntryRepository repository,
        IUnitOfWork unitOfWork,
        IProjectRepository projectRepository)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
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
    /// Запускает запись с указанным именем задачи и проектом.
    /// </summary>
    /// <param name="name">Имя задачи; обязательно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — запись без проекта.</param>
    public async Task Start(string name, Guid? projectId)
    {
        var now = _timeProvider.GetUtcNow();
        _session.Start(now);

        _active = TimeEntry.Start(Guid.NewGuid(), name, now, projectId);

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
    /// Переводит таймер в исходное состояние для следующей задачи.
    /// Запись, которой владел сценарий, отпускается: она уже завершена и сохранена,
    /// поэтому правка и переходы состояния ее больше не касаются.
    /// </summary>
    public Task NewTask()
    {
        _session.Reset();
        _active = null;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Переименовывает идущую или приостановленную запись.
    /// Имя меняется и в сохраненной строке, и в копии, которой владеет сценарий:
    /// иначе следующий переход состояния записал бы прежнее имя.
    /// </summary>
    /// <param name="name">Новое имя задачи; обязательно.</param>
    public async Task Rename(string name)
    {
        if (_active is null)
        {
            throw new InvalidTimeEntryException("Нельзя переименовать запись, которая не начата.");
        }

        _active = _active.Rename(name);

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

