using TimeTracker.Domain;

namespace TimeTracker.Application.Reports;

/// <summary>
/// Строка отчета по проекту: проект, суммарное время его задач и перечень задач.
/// </summary>
/// <param name="ProjectId">Идентификатор проекта; <c>null</c> — задачи без проекта.</param>
/// <param name="ProjectName">Имя проекта; для задач без проекта подставляется отдельная подпись.</param>
/// <param name="Total">Суммарное время завершенных задач проекта.</param>
/// <param name="TaskNames">Наименования задач проекта.</param>
/// <param name="Items">Задачи проекта в виде строк отчета по датам.</param>
public sealed record ProjectReportItem(
    Guid? ProjectId,
    string ProjectName,
    Duration Total,
    IReadOnlyList<string> TaskNames,
    IReadOnlyList<DateReportItem> Items);

/// <summary>
/// Строка отчета по тегу: имя тега, суммарное время задач с этим тегом и перечень задач.
/// </summary>
/// <param name="TagName">Имя тега.</param>
/// <param name="Total">Суммарное время завершенных задач с этим тегом.</param>
/// <param name="TaskNames">Наименования задач с этим тегом.</param>
/// <param name="Items">Задачи с этим тегом в виде строк отчета по датам.</param>
public sealed record TagReportItem(
    string TagName,
    Duration Total,
    IReadOnlyList<string> TaskNames,
    IReadOnlyList<DateReportItem> Items);

/// <summary>
/// Строка отчета по датам: задача, ее проект, моменты работы и время.
/// </summary>
/// <param name="TaskId">Идентификатор задачи.</param>
/// <param name="TaskName">Наименование задачи.</param>
/// <param name="ProjectName">Имя проекта; пустая строка, если проект не задан.</param>
/// <param name="StartedAt">Момент первого запуска; <c>null</c>, если задача не начиналась.</param>
/// <param name="FinishedAt">Момент завершения; <c>null</c>, если задача не завершена.</param>
/// <param name="Total">Время работы по задаче без пауз.</param>
public sealed record DateReportItem(
    Guid TaskId,
    string TaskName,
    string ProjectName,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    Duration Total);

/// <summary>
/// Отчет по проектам: строки и итог по всем строкам.
/// </summary>
/// <param name="Items">Строки отчета.</param>
/// <param name="Total">Суммарное время по всем строкам.</param>
public sealed record ProjectReport(IReadOnlyList<ProjectReportItem> Items, Duration Total);

/// <summary>
/// Отчет по тегам: строки и итог по всем строкам.
/// </summary>
/// <param name="Items">Строки отчета.</param>
/// <param name="Total">Суммарное время по всем строкам.</param>
public sealed record TagReport(IReadOnlyList<TagReportItem> Items, Duration Total);

/// <summary>
/// Отчет по датам: строки и итог по всем строкам.
/// </summary>
/// <param name="Items">Строки отчета.</param>
/// <param name="Total">Суммарное время по всем строкам.</param>
public sealed record DateReport(IReadOnlyList<DateReportItem> Items, Duration Total);
