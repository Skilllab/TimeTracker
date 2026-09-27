using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Входящий порт: управление задачами списка.
/// Одновременно выполняется не более одной задачи: запуск или продолжение выбранной
/// пристанавливает ту, которая шла до нее, поэтому запрет обеспечен в одном месте.
/// </summary>
public interface ITaskControl
{
    /// <summary>
    /// Создает задачу, которая еще не начиналась.
    /// </summary>
    /// <param name="name">Наименование задачи; обязательно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<Guid> CreateTaskAsync(string name, Guid? projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Запускает задачу, которая еще не начиналась.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Приостанавливает выполняемую задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task PauseAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Продолжает приостановленную задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ResumeAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершает выполняемую или приостановленную задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task FinishAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает завершенную задачу в работу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ReopenAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Переименовывает задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="name">Новое наименование задачи; обязательно.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RenameAsync(Guid taskId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Меняет проект задачи; <c>null</c> — задача без проекта.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — задача без проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ChangeProjectAsync(Guid taskId, Guid? projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Назначает задаче тег с указанным именем.
    /// Тег — это имя, а не отдельная сущность: справочника тегов нет,
    /// поэтому назначение добавляет имя задаче и не требует других изменений.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="tag">Имя тега.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task AddTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Снимает с задачи тег с указанным именем.
    /// Другие задачи не затрагиваются: имя тега хранится у самой задачи.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="tag">Имя тега.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RemoveTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Помечает задачу удаленной.
    /// Пометка не отвязывает задачу от проекта и не исключает ее из отчетов:
    /// задача остается в хранилище.
    /// Пометить можно только задачу, которая не выполняется сейчас.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Снимает с задачи пометку удаления.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RestoreTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет задачу из хранилища вместе с ее записями времени.
    /// Действие необратимо, поэтому выполняется только по явному выбору пользователя.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task DeletePermanentlyAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Приводит задачи к согласованному состоянию после запуска приложения:
    /// задача, оставшаяся выполняемой, переводится в паузу моментом последнего запуска,
    /// поэтому время работы закрытого приложения в длительность не попадает.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task RestoreAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Приостанавливает задачу, которая идет сейчас.
    /// Если ни одна задача не идет, ничего не происходит: вызов приходит от наблюдения за простоем,
    /// которое не знает состояние списка.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task PauseRunningAsync(CancellationToken cancellationToken = default);
}
