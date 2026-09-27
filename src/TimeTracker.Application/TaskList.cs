using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий списка задач: читает задачи, считает их время и упорядочивает для показа.
/// Сверху оказывается выполняемая задача, затем недавно запускавшиеся,
/// а завершенные уходят в конец списка.
/// Удаленные задачи читаются только по запросу и всегда идут последними:
/// они не участвуют в сортировке, но остаются видимыми и участвуют в отчетах.
/// Минипоиск по наименованию и по тегам только сужает список и не меняет состояние задач.
/// </summary>
public sealed class TaskList : ITaskList
{
    private readonly IWorkTaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="taskRepository">Исходящий порт хранилища задач.</param>
    /// <param name="projectRepository">Исходящий порт хранилища проектов.</param>
    /// <param name="timeProvider">Источник времени.</param>
    public TaskList(
        IWorkTaskRepository taskRepository,
        IProjectRepository projectRepository,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Возвращает задачи для показа с примененными фильтрами.
    /// Проекты читаются целиком: задача может ссылаться на проект, и без него
    /// у строки исчезли бы имя и цвет.
    /// Фильтры применяются совместно и только сужают список.
    /// </summary>
    /// <param name="search">Строка минипоиска по наименованию; <c>null</c> или пустая строка — без фильтра.</param>
    /// <param name="tagSearch">Строка минипоиска по тегам; <c>null</c> или пустая строка — без фильтра.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    /// <param name="includeDeleted">Признак того, что удаленные задачи тоже нужны.</param>
    public async Task<IReadOnlyList<TaskListItem>> GetAsync(
        string? search,
        string? tagSearch,
        Guid? projectId,
        CancellationToken cancellationToken = default,
        bool includeDeleted = false)
    {
        var now = _timeProvider.GetUtcNow();
        var tasks = await _taskRepository.GetAllAsync(includeDeleted, cancellationToken);
        var projects = await _projectRepository.GetAllAsync(cancellationToken);

        var projectsById = projects.ToDictionary(project => project.Id);
        var selected = tasks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var filter = search.Trim();

            selected = selected.Where(task =>
                task.Description.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(tagSearch))
        {
            var filter = tagSearch.Trim();

            selected = selected.Where(task => task.TagNames.Any(name =>
                name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)));
        }

        if (projectId is Guid selectedProjectId)
        {
            selected = selected.Where(task => task.ProjectId == selectedProjectId);
        }

        if (!includeDeleted)
        {
            selected = selected.Where(task => !task.IsDeleted);
        }

        return selected
            .Select(task => CreateItem(task, projectsById, now))
            .OrderBy(item => item.IsDeleted ? 1 : 0)
            .ThenBy(item => item.IsFinished ? 1 : 0)
            .ThenBy(item => item.IsRunning ? 0 : 1)
            .ThenByDescending(item => item.LastStartedAt)
            .ThenByDescending(item => item.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Создает строку списка по задаче.
    /// Время считается на переданный момент, поэтому у выполняемой задачи оно растет при каждом чтении списка.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="projectsById">Проекты, разложенные по идентификатору.</param>
    /// <param name="now">Момент, на который считается время.</param>
    private static TaskListItem CreateItem(
        WorkTask task,
        IReadOnlyDictionary<Guid, Project> projectsById,
        DateTimeOffset now)
    {
        Project? project = null;

        if (task.ProjectId is Guid projectId)
        {
            projectsById.TryGetValue(projectId, out project);
        }

        return new TaskListItem(
            task.Id,
            task.Description,
            task.ProjectId,
            project?.Name ?? string.Empty,
            project?.Color ?? string.Empty,
            task.Status,
            task.ElapsedAt(now),
            task.CreatedAt,
            task.StartedAt,
            task.LastStartedAt,
            task.FinishedAt,
            task.Tags,
            task.IsDeleted);
    }
}
