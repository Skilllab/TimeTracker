using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Хранилище проектов поверх базы данных.
/// </summary>
public sealed class ProjectRepository : IProjectRepository
{
    private readonly TimeTrackerDbContext _context;

    /// <summary>
    /// Создает хранилище.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public ProjectRepository(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Возвращает все проекты в порядке хранения.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
