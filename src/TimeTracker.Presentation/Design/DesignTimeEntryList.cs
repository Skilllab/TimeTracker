using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Presentation.Design;

/// <summary>
/// Заглушка входящего порта списка для дизайнера XAML: заполняет предпросмотр образцом записей.
/// </summary>
internal sealed class DesignTimeEntryList : ITimeEntryList
{
    /// <summary>
    /// Возвращает образец записей для предпросмотра.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<IReadOnlyList<TimeEntry>> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var start = new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero);

        IReadOnlyList<TimeEntry> entries = new List<TimeEntry>
        {
            new TimeEntry(Guid.NewGuid(), "Работа над отчетом", start, start.AddMinutes(45), 10, false, null),
            new TimeEntry(Guid.NewGuid(), "Созвон с клиентом", start.AddHours(1), start.AddHours(1).AddMinutes(30), 0, true, null)
        };

        return Task.FromResult(entries);
    }
}
