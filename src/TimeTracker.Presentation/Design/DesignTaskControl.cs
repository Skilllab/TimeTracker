using TimeTracker.Application;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта управления задачами для дизайнера XAML: ничего не меняет.
/// </summary>
internal sealed class DesignTaskControl : ITaskControl
{
    /// <summary>
    /// Создает задачу; в предпросмотре ничего не происходит, кроме выдачи идентификатора.
    /// </summary>
    /// <param name="name">Наименование задачи.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<Guid> CreateTaskAsync(string name, Guid? projectId, CancellationToken cancellationToken = default)
        => Task.FromResult(Guid.NewGuid());

    /// <summary>
    /// Запускает задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Приостанавливает задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task PauseAsync(Guid taskId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Продолжает задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task ResumeAsync(Guid taskId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Завершает задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task FinishAsync(Guid taskId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Возвращает задачу в работу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task ReopenAsync(Guid taskId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Переименовывает задачу.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="name">Новое наименование задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task RenameAsync(Guid taskId, string name, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Меняет проект задачи.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task ChangeProjectAsync(Guid taskId, Guid? projectId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Назначает задаче тег.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="tag">Имя тега.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task AddTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Снимает с задачи тег.
    /// </summary>
    /// <param name="taskId">Идентификатор задачи.</param>
    /// <param name="tag">Имя тега.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task RemoveTagAsync(Guid taskId, string tag, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Приводит задачи к согласованному состоянию.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task RestoreAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <summary>
    /// Приостанавливает идущую задачу.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task PauseRunningAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
