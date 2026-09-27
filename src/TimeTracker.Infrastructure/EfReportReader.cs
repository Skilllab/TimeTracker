using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Reports;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Чтение данных отчетов поверх базы данных.
/// Проекты читаются целиком: имя проекта нужно и в строках отчета, и в перечне задач.
/// </summary>
public sealed class EfReportReader : IReportReader
{
    private readonly TimeTrackerDbContext _context;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Создает хранилище отчетов.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="timeProvider">Источник времени.</param>
    public EfReportReader(TimeTrackerDbContext context, TimeProvider timeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Возвращает строки отчета по проектам.
    /// Задачи без проекта собираются в отдельную строку.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<ProjectReportItem>> ReadByProjectAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var tasks = await _context.Tasks.AsNoTracking().ToListAsync(cancellationToken);
        var projects = await _context.Projects.AsNoTracking().ToListAsync(cancellationToken);

        var projectsById = projects.ToDictionary(project => project.Id);
        var result = new List<ProjectReportItem>();

        foreach (var group in tasks.GroupBy(task => task.ProjectId))
        {
            var project = group.Key is Guid id && projectsById.TryGetValue(id, out var found) ? found : null;

            var items = group.Select(task => CreateItem(task, project?.Name ?? string.Empty, now)).ToList();

            result.Add(new ProjectReportItem(
                project?.Id,
                project?.Name ?? string.Empty,
                Sum(items),
                items.Select(item => item.TaskName).ToList(),
                items));
        }

        return result
            .OrderBy(item => item.ProjectName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Возвращает строки отчета по тегам.
    /// Одна задача попадает в строку каждого своего тега.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<TagReportItem>> ReadByTagAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var tasks = await _context.Tasks.AsNoTracking().ToListAsync(cancellationToken);
        var projects = await _context.Projects.AsNoTracking().ToListAsync(cancellationToken);

        var projectsById = projects.ToDictionary(project => project.Id);
        var names = tasks
            .SelectMany(task => task.TagNames)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var result = new List<TagReportItem>(names.Count);

        foreach (var name in names)
        {
            var items = tasks
                .Where(task => task.TagNames.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                .Select(task => CreateItem(task, ProjectName(task, projectsById), now))
                .ToList();

            result.Add(new TagReportItem(name, Sum(items), items.Select(item => item.TaskName).ToList(), items));
        }

        return result
            .OrderBy(item => item.TagName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Возвращает строки отчета по датам за период с необязательным отбором по проекту.
    /// Период проверяется по моменту начала задачи, а у задачи, которая еще не начиналась,
    /// по моменту создания: иначе созданная, но не запущенная задача в отчет не попадала бы.
    /// </summary>
    /// <param name="from">Начало периода включительно.</param>
    /// <param name="to">Конец периода включительно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<DateReportItem>> ReadByDateAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var tasks = await _context.Tasks.AsNoTracking().ToListAsync(cancellationToken);
        var projects = await _context.Projects.AsNoTracking().ToListAsync(cancellationToken);

        var projectsById = projects.ToDictionary(project => project.Id);

        var selected = tasks.Where(task =>
        {
            var moment = task.StartedAt ?? task.CreatedAt;

            return moment >= from && moment <= to;
        });

        if (projectId is Guid selectedProjectId)
        {
            selected = selected.Where(task => task.ProjectId == selectedProjectId);
        }

        return selected
            .Select(task => CreateItem(task, ProjectName(task, projectsById), now))
            .OrderByDescending(item => item.StartedAt)
            .ToList();
    }

    /// <summary>
    /// Создает строку отчета по задаче.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="projectName">Имя проекта; пустая строка, если проекта нет.</param>
    /// <param name="now">Момент, на который считается время.</param>
    private static DateReportItem CreateItem(WorkTask task, string projectName, DateTimeOffset now)
        => new(task.Id, task.Description, projectName, task.StartedAt, task.FinishedAt, task.ElapsedAt(now));

    /// <summary>
    /// Возвращает имя проекта задачи; пустая строка, если проекта нет.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="projectsById">Проекты, разложенные по идентификатору.</param>
    private static string ProjectName(WorkTask task, IReadOnlyDictionary<Guid, Project> projectsById)
        => task.ProjectId is Guid id && projectsById.TryGetValue(id, out var project)
            ? project.Name
            : string.Empty;

    /// <summary>
    /// Складывает время всех строк отчета: идущая и приостановленная задача тоже
    /// учитываются, поэтому итог показывает время, накопленное на момент чтения.
    /// </summary>
    /// <param name="items">Строки отчета.</param>
    private static Duration Sum(IEnumerable<DateReportItem> items)
    {
        var total = TimeSpan.Zero;

        foreach (var item in items)
        {
            total += item.Total.Value;
        }

        return total <= TimeSpan.Zero ? Duration.Zero : Duration.From(total);
    }
}
