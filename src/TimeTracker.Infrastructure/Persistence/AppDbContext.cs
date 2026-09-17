using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Clients;
using TimeTracker.Domain.Projects;
using TimeTracker.Domain.TimeTracking;

namespace TimeTracker.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext приложения
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Записи времени</summary>
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    /// <summary>Проекты</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>Клиенты</summary>
    public DbSet<Client> Clients => Set<Client>();

    /// <summary>
    /// Применить все IEntityTypeConfiguration&lt;T&gt; из сборки
    /// Infrastructure. Новые агрегаты подхватываются автоматически.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
