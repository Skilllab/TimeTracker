using TimeTracker.Domain;
using DomainTaskStatus = TimeTracker.Domain.TaskStatus;

namespace TimeTracker.Application;

/// <summary>
/// Строка списка задач: задача, ее проект, посчитанное время, теги и метки жизненного цикла.
/// Отдельная модель нужна потому, что экран показывает не только саму задачу,
/// но и имя с цветом проекта, а время считается на момент чтения списка.
/// Состояние берется из домена через отдельное имя: короткое имя <c>TaskStatus</c>
/// совпадает с типом из пространства задач платформы и без уточнения неоднозначно.
/// </summary>
/// <param name="Id">Идентификатор задачи.</param>
/// <param name="Description">Наименование задачи.</param>
/// <param name="ProjectId">Идентификатор проекта; <c>null</c>, если проект не задан.</param>
/// <param name="ProjectName">Имя проекта; пустая строка, если проект не задан.</param>
/// <param name="ProjectColor">Цвет проекта; пустая строка, если проект не задан.</param>
/// <param name="Status">Текущее состояние задачи.</param>
/// <param name="Elapsed">Время работы по задаче без пауз.</param>
/// <param name="CreatedAt">Момент создания задачи.</param>
/// <param name="StartedAt">Момент первого запуска; <c>null</c>, если задача не начиналась.</param>
/// <param name="LastStartedAt">Момент последнего запуска; <c>null</c>, если задача не начиналась.</param>
/// <param name="FinishedAt">Момент завершения; <c>null</c>, если задача не завершена.</param>
/// <param name="Tags">Имена тегов задачи одной строкой через разделитель.</param>
public sealed record TaskListItem(
    Guid Id,
    string Description,
    Guid? ProjectId,
    string ProjectName,
    string ProjectColor,
    DomainTaskStatus Status,
    Duration Elapsed,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? LastStartedAt,
    DateTimeOffset? FinishedAt,
    string Tags)
{
    /// <summary>Разделитель имен тегов в строке задачи.</summary>
    private const char TagSeparator = '|';

    /// <summary>Признак того, что задача еще не начиналась.</summary>
    public bool IsNotStarted => Status == DomainTaskStatus.NotStarted;

    /// <summary>Признак того, что задача выполняется.</summary>
    public bool IsRunning => Status == DomainTaskStatus.Running;

    /// <summary>Признак того, что задача приостановлена.</summary>
    public bool IsPaused => Status == DomainTaskStatus.Paused;

    /// <summary>Признак того, что задача завершена.</summary>
    public bool IsFinished => Status == DomainTaskStatus.Finished;

    /// <summary>
    /// Имена тегов задачи по отдельности, в том же порядке, что и в строке.
    /// Разбор нужен для показа тегов, отбора по тегам и отчетов.
    /// </summary>
    public IReadOnlyList<string> TagNames
        => string.IsNullOrEmpty(Tags)
            ? Array.Empty<string>()
            : Tags.Split(TagSeparator, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Метки жизненного цикла задачи: создана, начата и завершена.
    /// Неначатая задача имеет только метку создания; завершенная — все три.
    /// </summary>
    public IReadOnlyList<TaskLifecycle> Labels
    {
        get
        {
            var labels = new List<TaskLifecycle> { TaskLifecycle.Created };

            if (StartedAt is not null)
            {
                labels.Add(TaskLifecycle.Started);
            }

            if (Status == DomainTaskStatus.Finished)
            {
                labels.Add(TaskLifecycle.Finished);
            }

            return labels;
        }
    }

    /// <summary>
    /// Признак того, что задачу можно запустить или продолжить.
    /// Завершенная задача сначала возвращается в работу, поэтому запуск ей недоступен.
    /// </summary>
    public bool CanStart => Status is DomainTaskStatus.NotStarted or DomainTaskStatus.Paused;

    /// <summary>
    /// Признак того, что задачу можно приостановить.
    /// </summary>
    public bool CanPause => Status == DomainTaskStatus.Running;

    /// <summary>
    /// Признак того, что задачу можно завершить.
    /// </summary>
    public bool CanFinish => Status is DomainTaskStatus.Running or DomainTaskStatus.Paused;

    /// <summary>
    /// Признак того, что завершенную задачу можно вернуть в работу.
    /// </summary>
    public bool CanReopen => Status == DomainTaskStatus.Finished;

    /// <summary>
    /// Признак того, что задачу продолжает уже начатая работа.
    /// Для неначатой задачи действие называется запуском, для приостановленной — продолжением.
    /// </summary>
    public bool IsResume => Status == DomainTaskStatus.Paused;
}
