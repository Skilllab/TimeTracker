using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Контекст доступа к базе данных приложения.
/// </summary>
public sealed class TimeTrackerDbContext : DbContext
{
    /// <summary>
    /// Создает контекст.
    /// </summary>
    /// <param name="options">Настройки контекста.</param>
    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Записи времени.
    /// </summary>
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    /// <summary>
    /// Проекты.
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// Подключает конфигурации сущностей из этой сборки.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimeTrackerDbContext).Assembly);
    }
}
