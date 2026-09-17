using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Infrastructure.Persistence;

namespace TimeTracker.Infrastructure.Repositories;

/// <summary>
/// Реализация IUnitOfWork поверх AppDbContext.
/// </summary>
public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
