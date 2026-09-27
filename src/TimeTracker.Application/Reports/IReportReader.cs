namespace TimeTracker.Application.Reports;

/// <summary>
/// Исходящий порт: чтение данных для отчетов.
/// Отдельный порт нужен потому, что отчеты строятся по задачам и проектам
/// и не совпадают ни с одним из существующих способов чтения.
/// </summary>
public interface IReportReader
{
    /// <summary>
    /// Возвращает строки отчета по проектам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<ProjectReportItem>> ReadByProjectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает строки отчета по тегам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<TagReportItem>> ReadByTagAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает строки отчета по датам за указанный период.
    /// </summary>
    /// <param name="from">Начало периода включительно.</param>
    /// <param name="to">Конец периода включительно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task<IReadOnlyList<DateReportItem>> ReadByDateAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? projectId,
        CancellationToken cancellationToken = default);
}
