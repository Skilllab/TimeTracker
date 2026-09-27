using TimeTracker.Application.ReportExport;
using TimeTracker.Application.Reports;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта выгрузки для дизайнера XAML: файл не создается.
/// </summary>
internal sealed class DesignReportExporter : IReportExporter
{
    /// <summary>
    /// Ничего не записывает.
    /// </summary>
    /// <param name="path">Путь к файлу отчета.</param>
    /// <param name="items">Строки отчета.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task ExportAsync(
        string path,
        IReadOnlyList<DateReportItem> items,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
