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
            new TimeEntry(
                id: Guid.NewGuid(),
                description: "Работа над отчетом",
                startedAt: start,
                endedAt: start.AddMinutes(45),
                pausedSeconds: 10,
                pausedAt: null,
                isBillable: false,
                projectId: null),

            new TimeEntry(
                id: Guid.NewGuid(),
                description: "Созвон с клиентом",
                startedAt: start.AddHours(1),
                endedAt: start.AddHours(1).AddMinutes(30),
                pausedSeconds: 0,
                pausedAt: null,
                isBillable: true,
                projectId: null)
        };

        return Task.FromResult(entries);
    }
}
