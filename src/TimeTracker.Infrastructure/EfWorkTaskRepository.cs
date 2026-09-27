using Microsoft.EntityFrameworkCore;
using TimeTracker.Application;
using TimeTracker.Domain;
using DomainTaskStatus = TimeTracker.Domain.TaskStatus;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Хранилище задач поверх базы данных.
/// Теги хранятся строкой в самой задаче, поэтому отдельной таблицы связи нет.
/// Выборки идут без отслеживания, а обновление открепляет прежнюю версию задачи.
/// </summary>
public sealed class EfWorkTaskRepository : IWorkTaskRepository
{
    private readonly TimeTrackerDbContext _context;

    /// <summary>
    /// Создает хранилище.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public EfWorkTaskRepository(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Добавляет задачу.
    /// </summary>
    /// <param name="task">Добавляемая задача.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task AddAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
    }

    /// <summary>
    /// Возвращает задачи; удаленные попадают в результат только по запросу.
    /// </summary>
    /// <param name="includeDeleted">Признак того, что удаленные задачи тоже нужны.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<WorkTask>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var tasks = _context.Tasks.AsNoTracking();

        if (!includeDeleted)
        {
            tasks = tasks.Where(task => !task.IsDeleted);
        }

        return await tasks.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Возвращает задачу по идентификатору; <c>null</c>, если задачи нет.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    /// <summary>
    /// Возвращает выполняемую задачу; <c>null</c>, если ни одна задача не выполняется.
    /// Одновременно выполняется не более одной задачи, поэтому выборка возвращает не больше одной строки.
    /// </summary>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<WorkTask?> GetRunningAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Status == DomainTaskStatus.Running, cancellationToken);
    }

    /// <summary>
    /// Обновляет задачу.
    /// </summary>
    /// <param name="task">Обновляемая задача.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        var tracked = _context.ChangeTracker
            .Entries<WorkTask>()
            .FirstOrDefault(item => item.Entity.Id == task.Id);

        if (tracked is not null)
        {
            tracked.State = EntityState.Detached;
        }

        _context.Tasks.Update(task);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Удаляет задачу из хранилища вместе с ее записями времени.
    /// Записи уходят по связи задачи и записи, поэтому отдельных удалений не требуется.
    /// </summary>
    /// <param name="task">Удаляемая задача.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public Task DeletePermanentlyAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(task);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Возвращает задачи, созданные в указанном интервале.
    /// </summary>
    /// <param name="from">Начало интервала выборки.</param>
    /// <param name="to">Конец интервала выборки.</param>
    /// <param name="cancellationToken">Признак отмены операции.</param>
    public async Task<IReadOnlyList<WorkTask>> GetRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(task => task.CreatedAt >= from && task.CreatedAt < to)
            .OrderBy(task => task.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
