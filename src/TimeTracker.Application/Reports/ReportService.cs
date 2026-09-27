using TimeTracker.Domain;

namespace TimeTracker.Application.Reports;

/// <summary>
/// Сценарий отчетов: собирает строки отчета и считает итоги.
/// Итог складывается из времени всех строк отчета: идущая и приостановленная
/// задача тоже учитываются, поэтому итог показывает время на момент чтения.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly IReportReader _reader;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="reader">Исходящий порт чтения данных отчетов.</param>
    public ReportService(IReportReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    /// <summary>
    /// Возвращает отчет по проектам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<ProjectReport> GetByProjectAsync(CancellationToken cancellationToken = default)
    {
        var items = await _reader.ReadByProjectAsync(cancellationToken);

        return new ProjectReport(items, Sum(items.SelectMany(item => item.Items)));
    }

    /// <summary>
    /// Возвращает отчет по тегам.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<TagReport> GetByTagAsync(CancellationToken cancellationToken = default)
    {
        var items = await _reader.ReadByTagAsync(cancellationToken);

        return new TagReport(items, Sum(items.SelectMany(item => item.Items)));
    }

    /// <summary>
    /// Возвращает отчет по датам за период с необязательным отбором по проекту.
    /// </summary>
    /// <param name="from">Начало периода включительно.</param>
    /// <param name="to">Конец периода включительно.</param>
    /// <param name="projectId">Идентификатор проекта; <c>null</c> — все проекты.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<DateReport> GetByDateAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var items = await _reader.ReadByDateAsync(from, to, projectId, cancellationToken);

        return new DateReport(items, Sum(items));
    }

    /// <summary>
    /// Складывает время всех строк отчета.
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
