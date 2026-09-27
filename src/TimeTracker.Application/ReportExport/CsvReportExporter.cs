using System.Globalization;
using System.Text;
using TimeTracker.Application.Reports;

namespace TimeTracker.Application.ReportExport;

/// <summary>
/// Выгрузка отчета в CSV.
/// Разделитель — точка с запятой, а кодировка — UTF-8 с BOM: так русский текст
/// корректно открывается в табличных редакторах без настройки импорта.
/// Перевод строки — CRLF, потому что файл предназначен для Windows.
/// </summary>
public sealed class CsvReportExporter : IReportExporter
{
    private const string Separator = ";";
    private const string Header = "Задача;Проект;Время;Дата начала;Дата завершения";
    private const string MomentFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Записывает строки отчета в файл по указанному пути.
    /// </summary>
    /// <param name="path">Путь к создаваемому файлу.</param>
    /// <param name="items">Строки отчета; пустой список дает файл с одной строкой заголовков.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task ExportAsync(
        string path,
        IReadOnlyList<DateReportItem> items,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Путь к файлу отчета не может быть пустым.", nameof(path));
        }

        var builder = new StringBuilder();

        builder.Append(Header).Append("\r\n");

        foreach (var item in items ?? Array.Empty<DateReportItem>())
        {
            builder
                .Append(Escape(item.TaskName)).Append(Separator)
                .Append(Escape(item.ProjectName)).Append(Separator)
                .Append(Escape(item.Total.ToClockString())).Append(Separator)
                .Append(Escape(Format(item.StartedAt))).Append(Separator)
                .Append(Escape(Format(item.FinishedAt)))
                .Append("\r\n");
        }

        await File.WriteAllTextAsync(path, builder.ToString(), new UTF8Encoding(true), cancellationToken);
    }

    /// <summary>
    /// Форматирует момент времени; для отсутствующего момента возвращает пустую строку.
    /// Формат с секундами одинаков для всех строк, поэтому файл удобно сравнивать между выгрузками.
    /// </summary>
    /// <param name="moment">Момент времени; <c>null</c>, если момент не задан.</param>
    private static string Format(DateTimeOffset? moment)
        => moment is null
            ? string.Empty
            : moment.Value.ToLocalTime().ToString(MomentFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Экранирует значение для CSV.
    /// Значение с разделителем, кавычкой или переводом строки заключается в кавычки,
    /// а внутренние кавычки удваиваются — иначе строка распадется на лишние колонки.
    /// </summary>
    /// <param name="value">Значение колонки.</param>
    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;

        if (text.IndexOfAny(new[] { ';', '"', '\r', '\n' }) < 0)
        {
            return text;
        }

        return $"\"{text.Replace("\"", "\"\"")}\"";
    }
}
