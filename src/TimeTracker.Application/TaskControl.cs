using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий работы со списком задач: запускает, приостанавливает, завершает,
/// возвращает в работу и переименовывает задачи.
/// Одновременно выполняется не более одной задачи: перед запуском или продолжением
/// выбранной задачи сценарий приостанавливает ту, которая шла до нее,
/// поэтому правило обеспечено в одном месте и не зависит от экрана.
/// Каждый запуск и продолжение открывают запись о сегменте работы,
/// а приостановка и завершение закрывают ее.
/// </summary>
public sealed class TaskControl : ITaskControl
{
    private readonly IWorkTaskRepository _taskRepository;
    private readonly ITimeEntryRepository _entryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="taskRepository">Исходящий порт хранилища задач.</param>
    /// <param name="entryRepository">Исходящий порт хранилища записей.</param>
    /// <param name="unitOfWork">Исходящий порт фиксации изменений.</param>
    /// <param name="timeProvider">Источник времени.</param>
    public TaskControl(
        IWorkTaskRepository taskRepository,
        ITimeEntryRepository entryRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _entryRepository = entryRepository ?? throw new ArgumentNullException(nameof(entryRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Создает задачу, которая еще не начиналась.
    /// Имя проверяется доменом: пустое значение и значение из одних пробелов отвергаются.
    /// </summary>
    /// <param name="name">Наименование задачи; обязательно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<Guid> CreateTaskAsync(string name, Guid? projectId, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var task = WorkTask.Create(Guid.NewGuid(), name, projectId, now);

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.Id;
    }

    /// <summary>
    /// Запускает задачу: сначала останавливает ту, которая шла до нее,
    /// затем начинает отсчет и открывает новую запись о сегменте работы.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        await PauseRunningAsync(taskId, now, cancellationToken);

        var started = task.Start(now);

        await _taskRepository.UpdateAsync(started, cancellationToken);
        await AddSegmentAsync(started, now, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Приостанавливает выполняемую задачу: сегмент работы закрывается,
    /// поэтому время после приостановки в длительность не входит.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task PauseAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        await CloseOpenSegmentAsync(taskId, now, cancellationToken);
        await _taskRepository.UpdateAsync(task.Pause(now), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Продолжает приостановленную задачу: сначала останавливает ту, которая шла до нее,
    /// затем продолжает отсчет и открывает новую запись о сегменте работы.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ResumeAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        await PauseRunningAsync(taskId, now, cancellationToken);

        var resumed = task.Resume(now);

        await _taskRepository.UpdateAsync(resumed, cancellationToken);
        await AddSegmentAsync(resumed, now, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Завершает задачу: открытый сегмент работы закрывается,
    /// поэтому незакрытая пауза попадает в накопленное время пауз.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task FinishAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        await CloseOpenSegmentAsync(taskId, now, cancellationToken);
        await _taskRepository.UpdateAsync(task.Finish(now), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает завершенную задачу в работу.
    /// Задача становится приостановленной, поэтому отсчет начнется только после продолжения.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ReopenAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);
        var now = _timeProvider.GetUtcNow();

        await _taskRepository.UpdateAsync(task.Reopen(now), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Переименовывает задачу.
    /// Имя проверяется доменом, а уже закрытые сегменты работы сохраняют прежнее имя:
    /// это история, а не текущее состояние.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="name">Новое наименование задачи; обязательно.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RenameAsync(Guid taskId, string name, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.UpdateAsync(task.Rename(name), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Меняет проект задачи; <c>null</c> — задача без проекта.
    /// Уже закрытые сегменты работы сохраняют прежний проект: это история, а не текущее состояние.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — задача без проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ChangeProjectAsync(Guid taskId, Guid? projectId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.UpdateAsync(task.WithProject(projectId), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Назначает задаче тег с указанным именем.
    /// Имя проверяется доменом: пустое значение и повторы не создают второй тег.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="tag">Имя тега.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task AddTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.UpdateAsync(task.AddTag(tag), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Снимает с задачи тег с указанным именем.
    /// Другие задачи сохраняют тег: имя хранится у самой задачи, а не в общем списке.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="tag">Имя тега.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RemoveTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.UpdateAsync(task.RemoveTag(tag), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Помечает задачу удаленной.
    /// Пометка не отвязывает задачу от проекта и не исключает ее из отчетов:
    /// задача остается в хранилище вместе со своими записями времени.
    /// Пометить можно только задачу, которая не выполняется сейчас.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.UpdateAsync(task.Delete(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Снимает с задачи пометку удаления.
    /// Состояние, моменты, проект и теги сохраняются, поэтому восстановление
    /// не влияет на ход работы и на отчеты.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RestoreTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.UpdateAsync(task.Restore(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Удаляет задачу из хранилища вместе с ее записями времени.
    /// Записи времени уходят по связи задачи и записи, поэтому отдельных вызовов не требуется.
    /// Действие необратимо.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task DeletePermanentlyAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        await _taskRepository.DeletePermanentlyAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Приводит задачи к согласованному состоянию после запуска приложения:
    /// задача, оставшаяся выполняемой, переводится в паузу моментом последнего запуска.
    /// Так время закрытого приложения не попадает в длительность,
    /// потому что при следующем продолжении весь перерыв уходит в накопленные паузы.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        var running = await _taskRepository.GetRunningAsync(cancellationToken);

        if (running is null)
        {
            return;
        }

        var pausedAt = running.LastStartedAt ?? _timeProvider.GetUtcNow();

        await CloseOpenSegmentAsync(running.Id, pausedAt, cancellationToken);
        await _taskRepository.UpdateAsync(running.Pause(pausedAt), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Приостанавливает задачу, которая идет сейчас.
    /// Если ни одна задача не идет, ничего не происходит: вызов приходит от наблюдения за простоем.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task PauseRunningAsync(CancellationToken cancellationToken = default)
    {
        var running = await _taskRepository.GetRunningAsync(cancellationToken);

        if (running is null)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();

        await CloseOpenSegmentAsync(running.Id, now, cancellationToken);
        await _taskRepository.UpdateAsync(running.Pause(now), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает задачу по идентификатору; отсутствие задачи считается нарушением правила.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    private async Task<WorkTask> RequireTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task is null)
        {
            throw new InvalidTaskStateException("Задача не найдена.");
        }

        return task;
    }

    /// <summary>
    /// Приостанавливает выполняемую задачу, если это не та задача, которую запускают.
    /// Так выполняется правило: одновременно может идти не более одной задачи.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи, которая сейчас запускается.</param>
    /// <param name="now">Момент запуска; он же момент приостановки прежней задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    private async Task PauseRunningAsync(Guid taskId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var running = await _taskRepository.GetRunningAsync(cancellationToken);

        if (running is null || running.Id == taskId)
        {
            return;
        }

        await CloseOpenSegmentAsync(running.Id, now, cancellationToken);
        await _taskRepository.UpdateAsync(running.Pause(now), cancellationToken);
    }

    /// <summary>
    /// Закрывает открытую запись о сегменте работы задачи.
    /// Запись ищется среди незавершенных и закрывается только тогда, когда принадлежит задаче:
    /// незавершенная запись в приложении одна, потому что одновременно идет одна задача.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="now">Момент закрытия сегмента.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    private async Task CloseOpenSegmentAsync(Guid taskId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var segment = await _entryRepository.GetActiveAsync(cancellationToken);

        if (segment is null || segment.TaskId != taskId)
        {
            return;
        }

        await _entryRepository.UpdateAsync(segment.Close(now), cancellationToken);
    }

    /// <summary>
    /// Открывает новую запись о сегменте работы задачи.
    /// Запись сохраняет имя и проект задачи на момент запуска.
    /// </summary>
    /// <param name="task">Задача, для которой открывается сегмент.</param>
    /// <param name="now">Момент начала сегмента.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    private async Task AddSegmentAsync(WorkTask task, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var segment = TimeEntry.Start(Guid.NewGuid(), task.Description, now, task.ProjectId, task.Id);

        await _entryRepository.AddAsync(segment, cancellationToken);
    }
}
