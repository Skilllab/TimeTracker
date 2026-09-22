using TimeTracker.Domain;

namespace TimeTracker.Application;

/// <summary>
/// Сценарий чтения списка: вычисляет границы сегодняшнего дня и читает записи.
/// </summary>
public sealed class TimeEntryList : ITimeEntryList
{
    private readonly ITimeEntryRepository _repository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Создает сценарий.
    /// </summary>
    /// <param name="repository">Исходящий порт хранилища записей.</param>
    /// <param name="timeProvider">Источник времени.</param>
    public TimeEntryList(ITimeEntryRepository repository, TimeProvider timeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Возвращает записи за сегодня в порядке начала.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<IReadOnlyList<TimeEntry>> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var localNow = _timeProvider.GetLocalNow();
        var dayStart = new DateTimeOffset(localNow.Date, localNow.Offset);
        var dayEnd = dayStart.AddDays(1);

        return _repository.GetRangeAsync(dayStart, dayEnd, cancellationToken);
    }
}
