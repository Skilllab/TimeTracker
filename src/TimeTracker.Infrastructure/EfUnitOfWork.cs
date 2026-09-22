using System;
using System.Threading;
using System.Threading.Tasks;
using TimeTracker.Application;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Единица работы поверх контекста базы данных.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly TimeTrackerDbContext _context;

    /// <summary>
    /// Создает единицу работы.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public EfUnitOfWork(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Сохраняет изменения, сделанные за время работы.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
