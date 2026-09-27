namespace TimeTracker.Application.ReportExport;

using TimeTracker.Application.Reports;

/// <summary>
/// Входящий порт: выгрузка отчета в файл.
/// Порт принимает готовые строки отчета, поэтому экспортируется и полный список,
/// и отфильтрованный: отбор выполняется до выгрузки.
/// </summary>
public interface IReportExporter
{
    /// <summary>
    /// Записывает строки отчета в файл по указанному пути.
    /// </summary>
    /// <param name="path">Путь к создаваемому файлу.</param>
    /// <param name="items">Строки отчета; пустой список дает файл с одной строкой заголовков.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    Task ExportAsync(string path, IReadOnlyList<DateReportItem> items, CancellationToken cancellationToken = default);
}
