using TimeTracker.Application.Reports;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта отчетов для дизайнера XAML: пустые выборки без обращения к данным.
/// </summary>
internal sealed class DesignReportService : IReportService
{
    /// <summary>
    /// Возвращает пустой отчет по проектам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<ProjectReport> GetByProjectAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new ProjectReport(Array.Empty<ProjectReportItem>(), Duration.Zero));

    /// <summary>
    /// Возвращает пустой отчет по тегам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<TagReport> GetByTagAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new TagReport(Array.Empty<TagReportItem>(), Duration.Zero));

    /// <summary>
    /// Возвращает пустой отчет по датам.
    /// </summary>
    /// <param name="from">Начало периода.</param>
    /// <param name="to">Конец периода.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<DateReport> GetByDateAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? projectId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new DateReport(Array.Empty<DateReportItem>(), Duration.Zero));
}
