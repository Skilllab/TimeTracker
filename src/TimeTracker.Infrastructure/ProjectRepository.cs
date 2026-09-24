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
    /// Возвращает все проекты в порядке хранения, включая архивные.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает проект по идентификатору; <c>null</c>, если проекта нет.
    /// </summary>
    /// <param name="id">Идентификатор проекта.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);
    }

    /// <summary>
    /// Добавляет проект.
    /// </summary>
    /// <param name="project">Добавляемый проект.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }

    /// <summary>
    /// Обновляет проект.
    /// Проект неизменяем, поэтому сущность с тем же идентификатором
    /// могла уже отслеживаться контекстом: перед обновлением
    /// прежняя версия открепляется, иначе сохранение упрется в конфликт ключа.
    /// </summary>
    /// <param name="project">Обновляемый проект.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        var tracked = _context.ChangeTracker
            .Entries<Project>()
            .FirstOrDefault(item => item.Entity.Id == project.Id);

        if (tracked is not null)
        {
            tracked.State = EntityState.Detached;
        }

        _context.Projects.Update(project);

        return Task.CompletedTask;
    }
}
