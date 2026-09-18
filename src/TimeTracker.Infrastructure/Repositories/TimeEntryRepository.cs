using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Domain.TimeTracking;
using TimeTracker.Infrastructure.Persistence;

namespace TimeTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core реализация ITimeEntryRepository
/// </summary>
public sealed class TimeEntryRepository(AppDbContext db) : ITimeEntryRepository
{
    public Task<TimeEntry?> GetByIdAsync(TimeEntryId id, CancellationToken ct = default)
        => db.TimeEntries.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<TimeEntry?> GetRunningAsync(CancellationToken ct = default)
        => db.TimeEntries
            .Where(e => EF.Property<long?>(e, "Range_End_Ticks") == null)
            .OrderByDescending(e => EF.Property<long>(e, "Range_Start_Ticks"))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TimeEntry>> GetByDateRangeAsync(
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken ct = default)
        => await db.TimeEntries
            .Where(e => EF.Property<long>(e, "Range_Start_Ticks") >= rangeStart.UtcTicks
                        && EF.Property<long>(e, "Range_Start_Ticks") < rangeEnd.UtcTicks)
            .OrderByDescending(e => EF.Property<long>(e, "Range_Start_Ticks"))
            .ToListAsync(ct);

    public async Task AddAsync(TimeEntry entry, CancellationToken ct = default)
        => await db.TimeEntries.AddAsync(entry, ct);

    public void Remove(TimeEntry entry) => db.TimeEntries.Remove(entry);
}
