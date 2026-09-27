namespace TimeTracker.Application.Reports;

/// <summary>
/// Входящий порт: построение отчетов для окна отчетов.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Возвращает отчет по проектам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<ProjectReport> GetByProjectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает отчет по тегам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<TagReport> GetByTagAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает отчет по датам за период с необязательным отбором по проекту.
    /// </summary>
    /// <param name="from">Начало периода включительно.</param>
    /// <param name="to">Конец периода включительно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<DateReport> GetByDateAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? projectId,
        CancellationToken cancellationToken = default);
}
