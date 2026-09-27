using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Справочник тегов поверх базы данных.
/// Выборки идут без отслеживания, а обновление открепляет прежнюю версию тега:
/// так переименование тега не конфликтует с уже прочитанными строками.
/// </summary>
public sealed class EfTagRepository : ITagRepository
{
    private readonly TimeTrackerDbContext _context;

    /// <summary>
    /// Создает справочник.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public EfTagRepository(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Возвращает все теги справочника.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Добавляет тег в справочник.
    /// </summary>
    /// <param name="tag">Добавляемый тег.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tag, cancellationToken);
    }

    /// <summary>
    /// Обновляет тег справочника.
    /// </summary>
    /// <param name="tag">Обновляемый тег.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        var tracked = _context.ChangeTracker
            .Entries<Tag>()
            .FirstOrDefault(item => item.Entity.Id == tag.Id);

        if (tracked is not null)
        {
            tracked.State = EntityState.Detached;
        }

        _context.Tags.Update(tag);

        return Task.CompletedTask;
    }
}
