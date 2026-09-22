using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Хранилище записей времени поверх базы данных.
/// </summary>
public sealed class TimeEntryRepository : ITimeEntryRepository
{
    private readonly TimeTrackerDbContext _context;

    /// <summary>
    /// Создает хранилище.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public TimeEntryRepository(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Добавляет запись времени.
    /// </summary>
    /// <param name="entry">Добавляемая запись.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.TimeEntries.AddAsync(entry, cancellationToken);
    }

    /// <summary>
    /// Возвращает идущую запись; <c>null</c>, если незавершенных записей нет.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<TimeEntry?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return _context.TimeEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.EndedAt == null, cancellationToken);
    }

    /// <summary>
    /// Возвращает записи, начавшиеся в указанном интервале, в порядке начала.
    /// </summary>
    /// <param name="from">Начало интервала выборки.</param>
    /// <param name="to">Конец интервала выборки.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<TimeEntry>> GetRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _context.TimeEntries
            .AsNoTracking()
            .Where(entry => entry.StartedAt >= from && entry.StartedAt < to)
            .OrderBy(entry => entry.StartedAt)
            .ToListAsync(cancellationToken);
    }
}
