using Microsoft.EntityFrameworkCore;
using TimeTracker.Application.Abstractions.Persistence;
using TimeTracker.Domain.Projects;
using TimeTracker.Infrastructure.Persistence;

namespace TimeTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core реализация IProjectRepository
/// </summary>
public sealed class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct = default)
        => db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
        => await db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Project>> GetActiveAsync(CancellationToken ct = default)
        => await db.Projects
            .AsNoTracking()
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Project project, CancellationToken ct = default)
        => await db.Projects.AddAsync(project, ct);

    public void Remove(Project project) => db.Projects.Remove(project);
}
