using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TimeTracker.Infrastructure;

/// <summary>
/// Создает контекст для команд работы с миграциями.
/// </summary>
public sealed class TimeTrackerDbContextFactory : IDesignTimeDbContextFactory<TimeTrackerDbContext>
{
    /// <summary>
    /// Создает контекст в папке данных приложения.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    public TimeTrackerDbContext CreateDbContext(string[] args)
    {
        var paths = new AppDataPaths();
        Directory.CreateDirectory(paths.DataDirectory);

        var databasePath = Path.Combine(paths.DataDirectory, "timetracker.db");
        var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new TimeTrackerDbContext(options);
    }
}
